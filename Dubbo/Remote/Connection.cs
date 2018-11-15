using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Dubbo.Remote
{
    public class Connection
    {
        private const int DefaultTimeout = 30000;
        private static ILog log = LogManager.GetLogger(typeof(Connection));

        private readonly string _host;
        private readonly int _port;
        private readonly BlockingCollection<Request> _sendQueue;
        private TcpClient _client;
        private NetworkStream _rwStream;
        private Task _sendTask;
        private Task _receiveTask;
        private bool _isClosed;
        private AutoResetEvent _conStateEvent;


        public bool IsConnected => !_isClosed && _client != null && _client.Connected && _rwStream != null;
        public DateTime LastReadTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string Address => $"{_host}:{_port.ToString()}";

        public Connection(string host, int port)
        {
            _host = host;
            _port = port;
            _sendQueue = new BlockingCollection<Request>();
            _conStateEvent = new AutoResetEvent(false);
        }

        private void InitClient()
        {
            _client = new TcpClient
            {
                NoDelay = true,
                LingerState = new LingerOption(true, 0),
                SendTimeout = DefaultTimeout
            };
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _client.Connect(_host, _port);
            _rwStream = _client.GetStream();
        }
        private void CloseClient()
        {
            if (!IsConnected)
            {
                return;
            }
            _rwStream.Close();
            _client.Close();
            _rwStream = null;
            _client = null;
        }

        private void Start()
        {
            _sendTask = Task.Factory.StartNew(() =>
             {

                 while (!_isClosed && !_sendQueue.IsAddingCompleted)
                 {
                     Request request = null;
                     try
                     {
                         request = _sendQueue.Take();
                         Codec.EncodeRequest(request, _rwStream);
                         _rwStream.Flush();
                         LastWriteTime = DateTime.UtcNow;
                     }
                     catch (Exception ex)
                     {
                         switch (ex)
                         {
                             case IOException _:
                             case ObjectDisposedException _:
                                 RequestTask task;
                                 if (request != null && (task = RequestTasks.GetRequestTask(request.RequestId)) != null)
                                 {
                                     task.Task.TrySetException(new RemotingException($"message can not send, because connection is closed . address:{_host}:{_port.ToString()}", ex));
                                 }
                                 Reconect();
                                 log.Warn("An error happend when send message", ex);
                                 break;
                             case InvalidOperationException _:
                                 log.Warn("the connection has been closed", ex);
                                 break;
                         }
                     }
                 }

                 while (_sendQueue.TryTake(out var request))
                 {
                     var task = RequestTasks.GetRequestTask(request.RequestId);
                     task?.Task.TrySetException(new RemotingException($"message can not send, because connection is closed . address:{_host}:{_port.ToString()}"));

                 }
             }, TaskCreationOptions.LongRunning);
            _receiveTask = Task.Factory.StartNew(() =>
             {
                 while (!_isClosed)
                 {
                     try
                     {
                         var response = Codec.DecodeResponse(_rwStream);
                         if (response.IsEvent)
                         {
                             log.Debug("Receive HeartBeat Response");
                         }
                         LastReadTime = DateTime.UtcNow;
                     }
                     catch (Exception ex)
                     {
                         if (ex is IOException || ex is ObjectDisposedException)
                         {
                             if (_isClosed)
                             {
                                 return;
                             }
                             Reconect();
                         }
                         log.Warn("An error happend when receive message", ex);
                     }
                 }
             }, TaskCreationOptions.LongRunning);
        }

        public void Reconect()
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    CloseClient();
                    InitClient();
                }
                finally
                {
                    Monitor.Exit(this);
                    _conStateEvent.Set();
                }
            }
            else
            {
                _conStateEvent.WaitOne();
            }

        }

        public void Connect()
        {
            lock (this)
            {
                if (IsConnected)
                {
                    return;
                }
                InitClient();
                Start();
            }
        }

        public void Close()
        {
            lock (this)
            {
                if (!IsConnected)
                {
                    return;
                }
                _isClosed = true;
                _sendQueue.CompleteAdding();
                if (IsConnected)
                {
                    CloseClient();
                }
                Task.WaitAll(_sendTask, _receiveTask);
            }
        }

        public Task<Response> Send(Request request)
        {
            if (_sendQueue.IsAddingCompleted)
            {
                return Task.FromCanceled<Response>(CancellationToken.None);
            }
            var resultTask = RequestTasks.NewRequest(request);
            if (_sendQueue.TryAdd(request))
            {
                return resultTask;
            }
            RequestTasks.GetRequestTask(request.RequestId);
            return Task.FromCanceled<Response>(CancellationToken.None);
        }


        public override bool Equals(object obj)
        {
            if (obj is Connection self)
            {
                return self == this || self._host == this._host && self._port == this._port;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (_host.GetHashCode() << 16 | _port).GetHashCode();
        }

        public override string ToString()
        {
            return Address;
        }
    }
}