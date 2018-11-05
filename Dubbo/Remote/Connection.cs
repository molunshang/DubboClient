using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Dubbo.Remote
{
    public class Connection
    {
        private const int DefaultTimeout = 30000;
        private static ILog log = LogManager.GetLogger(typeof(Connection));

        private readonly string _host;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _rwStream;
        private ConcurrentDictionary<long, TaskCompletionSource<Response>> waitingTasks;
        private BlockingCollection<Request> sendQueue = new BlockingCollection<Request>();

        public bool IsConnected => _client.Connected && _rwStream != null;
        public DateTime LastReadTime { get; set; }
        public DateTime LastWriteTime { get; set; }

        public Connection(string host, int port)
        {
            _host = host;
            _port = port;
            waitingTasks = new ConcurrentDictionary<long, TaskCompletionSource<Response>>();
            _client = new TcpClient
            {
                NoDelay = true,
                LingerState = new LingerOption(true, 0),
                SendTimeout = DefaultTimeout
            };
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        private void Start()
        {
            Task.Factory.StartNew(() =>
            {

                while (!sendQueue.IsCompleted)
                {
                    try
                    {
                        var request = sendQueue.Take();
                        Codec.EncodeRequest(request, _rwStream);
                        _rwStream.Flush();
                        LastWriteTime = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException || ex is ObjectDisposedException)
                        {
                            Close();
                            Reconect().Wait();
                            //todo 重新建立链接
                        }
                        log.Warn("An error happend when send message", ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        var response = Codec.DecodeResponse(_rwStream);
                        if (waitingTasks.TryGetValue(response.ResponseId, out var task))
                        {
                            if (response.IsEvent)
                            {
                                log.Debug("Receive HeartBeat Response");
                            }
                            else
                            {
                                task.TrySetResult(response);
                            }
                        }
                        LastReadTime = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException || ex is ObjectDisposedException)
                        {
                            Close();
                            Reconect().Wait();
                            //todo 重新建立链接
                        }
                        log.Warn("An error happend when send message", ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task Reconect()
        {
            throw new NotImplementedException();
        }

        public async Task Connect()
        {
            await _client.ConnectAsync(_host, _port);
            _rwStream = _client.GetStream();
            Start();
        }

        public void Close()
        {
            if (IsConnected)
            {
                _client.Close();
            }
        }

        public Task<Response> Send(Request request)
        {
            var future = new TaskCompletionSource<Response>();
            waitingTasks.TryAdd(request.RequestId, future);
            sendQueue.Add(request);
            return future.Task;
        }

        public override bool Equals(object obj)
        {
            if (obj is Connection self)
            {
                return self == this || (self._host == this._host && self._port == this._port);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return $"{_host}:{_port.ToString()}".GetHashCode();
        }
    }
}