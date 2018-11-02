using Dubbo.Config;
using org.apache.zookeeper;
using Rabbit.Zookeeper;
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
        private readonly IZookeeperClient client;
        private volatile ZooKeeper zooKeeper;
        public ZookeeperRegistry(IZookeeperClient zkClient)
        {
            client = zkClient;
            zooKeeper = client.ZooKeeper;
            client.SubscribeStatusChange((self, args) =>
            {
                if (self.ZooKeeper == zooKeeper)
                {
                    return Task.CompletedTask;
                }

                zooKeeper = self.ZooKeeper;
                return Recover();
            });
        }

        private async Task CreatePath(string path, bool isParent)
        {
            if (isParent)
            {
                var exists = await client.ExistsAsync(path);
                if (exists)
                {
                    return;
                }
            }

            var index = path.LastIndexOf('/');
            if (index > 0)
            {
                await CreatePath(path.Substring(0, index), true);
            }

            await client.CreateAsync(path, null, ZooDefs.Ids.OPEN_ACL_UNSAFE,
                isParent ? CreateMode.PERSISTENT : CreateMode.EPHEMERAL);
        }

        protected override async Task DoRegister(ServiceConfig config)
        {
            var path =
                $"/dubbo/{config.ServiceName}/consumers/{HttpUtility.UrlEncode(config.ToServiceUrl(), Encoding.UTF8)}";
            await CreatePath(path, false);
            Log.InfoFormat("register {0} success.", path);
        }

        protected override async Task DoSubscribe(ServiceConfig config, Action<IList<ServiceConfig>> onChange)
        {
            var path = $"/dubbo/{config.ServiceName}/providers";
            await CreatePath(path, true);
            var childs = await client.SubscribeChildrenChange(path, ((zk, args) =>
            {
                var configs = args.CurrentChildrens.Select(child =>
                    ServiceConfig.ParseServiceUrl(HttpUtility.UrlDecode(child, Encoding.UTF8))).ToArray();
                onChange(configs);
                return Task.CompletedTask;
            }));
            onChange(childs.Select(child => ServiceConfig.ParseServiceUrl(HttpUtility.UrlDecode(child, Encoding.UTF8)))
                .ToArray());
            Log.InfoFormat("subscribe {0} success.", path);
        }
    }
}