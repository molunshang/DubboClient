using Dubbo.Config;
using Dubbo.Remote;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Dubbo
{
    public class DubboInvoker
    {
        private ILog _log = LogManager.GetLogger(typeof(DubboInvoker));
        private volatile bool forbidden = false;
        private IList<Connection> _connections;
        private int _index;
        private ConnectionFactory connectionFactory = new ConnectionFactory();
        private IReadOnlyDictionary<MethodInfo, InvokeContext> _methodDictionary;

        private void DestoryConnections(ICollection<Connection> connections)
        {
            if (connections == null || connections.Count <= 0)
                return;
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
        }
        protected Connection SelectConnection()
        {
            var connections = _connections;
            return connections[(Interlocked.Increment(ref _index) & int.MaxValue) % _connections.Count];
        }

        public DubboInvoker(IReadOnlyDictionary<MethodInfo, InvokeContext> methodDictionary)
        {
            _methodDictionary = methodDictionary;
        }

        public void RefreshConnection(ISet<ServiceConfig> configs)
        {
            lock (this)
            {
                var olds = _connections;
                if (configs.Count <= 0)
                {
                    forbidden = true;
                    DestoryConnections(olds);
                    return;
                }

                if (olds == null)
                {
                    _connections = configs.Select(config => connectionFactory.CreateConnection(config.Host, config.Port)).ToArray();
                }
                else
                {
                    var news = new List<Connection>(configs.Count);
                    var existsConDic = olds.ToDictionary(con => con.Address, con => con);
                    foreach (var config in configs)
                    {
                        if (existsConDic.TryGetValue(config.Address, out var connection))
                        {
                            news.Add(connection);
                            existsConDic.Remove(config.Address);
                        }
                        else
                        {
                            news.Add(connectionFactory.CreateConnection(config.Host, config.Port));
                        }
                    }
                    _connections = news;
                    if (existsConDic.Count > 0)
                        DestoryConnections(existsConDic.Values);
                }
            }

        }



        public Task<Response> Invoke(MethodInfo targetMethod, object[] args)
        {
            if (forbidden)
            {
                throw new InvalidOperationException($"unknow method {targetMethod.Name}");
            }
            if (!_methodDictionary.TryGetValue(targetMethod, out var context))
            {
                throw new InvalidOperationException($"unknow method {targetMethod.Name}");
            }

            var request = new Request
            {
                IsTwoWay = true,
                MethodName = context.Method,
                Arguments = args,
                Service = context.Service,
                ParameterTypeInfo = context.ParameterTypeInfo,
                ReturnType = context.ReturnType,
                Version = context.Version,
                Attachments =
                {
                    ["group"] = context.Group,
                    ["timeout"] = context.Timeout > 0 ? context.Timeout.ToString() : "60000"
                }
            };
            var connection = SelectConnection();
            return connection.Send(request);
        }
    }
}
