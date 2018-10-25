using Dubbo.Config;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Dubbo.Registry
{
    public class ZookeeperRegistry : AbstractRegistry
    {
        private readonly ZooKeeper client;
        public ZookeeperRegistry(ZooKeeper zkClient)
        {
            client = zkClient;
        }

        async Task CreatePath(string path, bool isParent)
        {
            if (isParent)
            {
                var stat = await client.existsAsync(path);
                if (stat != null)
                {
                    return;
                }
            }

            var index = path.LastIndexOf('/');
            if (index > 0)
            {
                await CreatePath(path.Substring(0, index), true);
            }

            await client.createAsync(path, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, isParent ? CreateMode.PERSISTENT : CreateMode.EPHEMERAL).ContinueWith(
                t =>
                {
                    if (t.Exception != null && t.Exception.InnerExceptions.Any(ex => ex is KeeperException.NodeExistsException))
                    {
                        return string.Empty;
                    }
                    return t.Result;
                });
        }

        protected override void DoRegister(ServiceConfig config)
        {
            var path = $"/dubbo/{config.ServiceName}/consumers/{HttpUtility.UrlEncode(config.ToServiceUrl(), Encoding.UTF8)}";
            CreatePath(path, false).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("register to zookeeper fail ");
                }
            });
        }

        protected override void DoSubscribe(ServiceConfig config, Action<IList<ServiceConfig>> onChange)
        {
            var path = $"/dubbo/{config.ServiceName}/providers";
            CreatePath(path, true).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("error happen when subscribe");
                    return;
                }

                var childWatcher = ZookeeperWatcherWrapper.ProcessChange((state, self) =>
                {
                    switch (state.get_Type())
                    {
                        case Watcher.Event.EventType.NodeCreated:
                        case Watcher.Event.EventType.NodeDeleted:
                        case Watcher.Event.EventType.NodeChildrenChanged:
                            client.existsAsync(path, self).ContinueWith(stat =>
                            {
                                if (stat.IsFaulted)
                                {
                                    return;
                                }

                                if (stat.Result == null)
                                {
                                    return;
                                }
                                client.getChildrenAsync(path, self).ContinueWith(ct =>
                                {
                                    if (ct.IsFaulted)
                                    {
                                        Console.WriteLine("error happen when getChildrenAsync");
                                        return;
                                    }

                                    onChange(ct.Result.Children.Select(ch =>
                                    {
                                        Console.WriteLine(ch);
                                        var raw = HttpUtility.UrlDecode(ch, Encoding.UTF8);
                                        return new ServiceConfig(raw.Split('&').Select(line => line.Split('=')).ToDictionary(key => key[0], val => val[1]));
                                    }).ToArray());
                                });
                            });
                            break;
                    }
                });
                client.getChildrenAsync(path, childWatcher).ContinueWith(ct =>
                {
                    if (ct.IsFaulted)
                    {
                        Console.WriteLine("error happen when getChildrenAsync");
                        return;
                    }

                    onChange(ct.Result.Children.Select(ch =>
                    {
                        Console.WriteLine(ch);
                        var raw = HttpUtility.UrlDecode(ch, Encoding.UTF8);
                        return new ServiceConfig(raw.Split('&').Select(line => line.Split('=')).ToDictionary(key => key[0], val => val[1]));
                    }).ToArray());
                });
            });
        }
    }
}