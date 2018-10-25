using org.apache.zookeeper;
using System;
using System.Threading.Tasks;

namespace Dubbo.Registry
{
    public class ZookeeperWatcherWrapper : Watcher
    {
        private readonly Action<WatchedEvent, ZookeeperWatcherWrapper> _processAction;

        private ZookeeperWatcherWrapper(Action<WatchedEvent, ZookeeperWatcherWrapper> process)
        {
            _processAction = process;
        }
        public override Task process(WatchedEvent @event)
        {
            _processAction(@event, this);
            return Task.CompletedTask;
        }

        public static Watcher ProcessChange(Action<WatchedEvent, ZookeeperWatcherWrapper> process)
        {
            return new ZookeeperWatcherWrapper(process);
        }
    }
}