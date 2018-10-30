using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Dubbo
{
    public class Connection
    {
        private const int DefaultTimeout = 30000;

        private long _lastReadTime;
        private long _lastWriteTime;
        private readonly string _host;
        private readonly int _port;
        private readonly Timer _heartBeatTask;
        private readonly TcpClient _client;
        private NetworkStream _rwStream;
        private ConcurrentDictionary<long, TaskCompletionSource<Response>> waitingTasks;

        public bool IsConnected => _client.Connected && _rwStream != null;

        public Connection(string host, int port)
        {
            _host = host;
            _port = port;
            waitingTasks = new ConcurrentDictionary<long, TaskCompletionSource<Response>>();
            _heartBeatTask = new Timer(HeartBeat);
            _client = new TcpClient
            {
                NoDelay = true,
                LingerState = new LingerOption(true, 0),
                SendTimeout = DefaultTimeout
            };
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
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

        private void HeartBeat(object self)
        {
            if (IsConnected && (DateTime.Now.Ticks - _lastReadTime >= TimeSpan.FromMilliseconds(DefaultTimeout).Ticks || DateTime.Now.Ticks - _lastWriteTime >= TimeSpan.FromMilliseconds(DefaultTimeout).Ticks))
            {
                var heartBeatRequest = new Request
                {
                    IsEvent = true,
                    IsTwoWay = true
                };
                Console.WriteLine("send heartbeat");
                Send(heartBeatRequest);
            }

            StartHeartBeat();
        }

        private void StartHeartBeat()
        {
            _heartBeatTask.Change(DefaultTimeout, Timeout.Infinite);
        }

        private void StartResponseLoop()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        var response = Codec.DecodeResponse(_rwStream, typeof(object));
                        if (waitingTasks.TryGetValue(response.ResponseId, out var task))
                        {
                            if (response.IsEvent)
                            {
                                Console.WriteLine("is heartbeat");
                            }
                            else
                            {
                                task.TrySetResult(response);
                                Console.WriteLine(response.Result);
                            }
                        }
                        _lastReadTime = DateTime.Now.Ticks;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task Connect()
        {
            await _client.ConnectAsync(_host, _port);
            _rwStream = _client.GetStream();
            StartHeartBeat();
            StartResponseLoop();
        }

        public void Close()
        {
            if (IsConnected)
            {
                _client.Close();
            }
            _heartBeatTask.Dispose();
        }

        public Task<Response> Send(Request request)
        {
            var future = new TaskCompletionSource<Response>();
            waitingTasks.TryAdd(request.RequestId, future);
            Codec.EncodeRequest(request, _rwStream);
            _rwStream.FlushAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    waitingTasks.TryRemove(request.RequestId, out future);
                    future.SetException(t.Exception);
                    return;
                }
                _lastWriteTime = DateTime.Now.Ticks;
            });
            return future.Task;
        }
    }
}