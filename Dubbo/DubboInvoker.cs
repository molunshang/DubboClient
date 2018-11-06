using Dubbo.Config;
using Dubbo.Remote;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dubbo
{
    public class DubboInvoker
    {
        private ILog _log = LogManager.GetLogger(typeof(DubboInvoker));
        private volatile bool forbidden = false;
        private IList<Connection> _connections;
        private int _index;
        private ConnectionFactory connectionFactory;
        public void RefreshConnection(ISet<ServiceConfig> configs)
        {
            lock (this)
            {
                if (configs.Count <= 0)
                {
                    forbidden = true;
                    foreach (var con in _connections)
                    {
                        try
                        {
                            con.Close();
                        }
                        catch (Exception ex)
                        {
                            _log.Warn($"Failed to close connection {con}", ex);
                        }
                    }
                    return;
                }

                var news = new List<Connection>(configs.Count);
                if (_connections == null)
                {
                    news.AddRange(configs.Select(config => connectionFactory.CreateConnection(config.Host, config.Port)));
                    _connections = news;
                }
                else
                {
                    var olds = new HashSet<Connection>(_connections);

                }
            }

        }

        public Connection SelectConnection()
        {
            return _connections[(Interlocked.Increment(ref _index) & int.MaxValue) % _connections.Count];
        }
    }
}
