using Dubbo.Remote;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dubbo
{
    public class ConnectionFactory
    {
        private readonly ConcurrentDictionary<string, Connection> _connections = new ConcurrentDictionary<string, Connection>();
        private readonly Timer _heartBeaTimer;
        private readonly TimeSpan _heartBeatPeriod;
        private readonly ILog _log = LogManager.GetLogger(typeof(ConnectionFactory));

        public ConnectionFactory()
        {
            _heartBeatPeriod = TimeSpan.FromSeconds(30);
            _heartBeaTimer = new Timer(CheckAlive, this, _heartBeatPeriod, Timeout.InfiniteTimeSpan);
        }

        private void CheckAlive(object args)
        {

            try
            {
                foreach (var connection in _connections.Values)
                {
                    if (!connection.IsConnected || DateTime.UtcNow - connection.LastReadTime < _heartBeatPeriod ||
                        DateTime.UtcNow - connection.LastWriteTime < _heartBeatPeriod)
                    {
                        continue;
                    }
                    connection.Send(new Request() { IsEvent = true, IsTwoWay = true }).ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            _log.Error("An error was happend when do heartbeat", t.Exception);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _log.Error("An error was happend when do heartbeat", ex);
            }
            finally
            {
                _heartBeaTimer.Change(_heartBeatPeriod, Timeout.InfiniteTimeSpan);
            }


        }
        public Connection CreateConnection(string host, int port)
        {
            var con = _connections.GetOrAdd($"{host}:{port}", new Connection(host, port));
            con.Connect();
            return con;
        }
    }
}
