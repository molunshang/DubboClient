using Dubbo.Collection;
using Dubbo.Config;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dubbo.Registry
{
    public abstract class AbstractRegistry
    {
        private readonly ConcurrentSet<ServiceConfig> failRegisterConfigs = new ConcurrentSet<ServiceConfig>();
        private readonly ConcurrentSet<ServiceConfig> successRegisterConfigs = new ConcurrentSet<ServiceConfig>();
        private readonly ConcurrentDictionary<ServiceConfig, ISet<Action<IList<ServiceConfig>>>> failSubscribeConfigs = new ConcurrentDictionary<ServiceConfig, ISet<Action<IList<ServiceConfig>>>>();
        private readonly ConcurrentDictionary<ServiceConfig, ISet<Action<IList<ServiceConfig>>>> successSubscribeConfigs = new ConcurrentDictionary<ServiceConfig, ISet<Action<IList<ServiceConfig>>>>();

        private readonly Timer retryTask;
        protected ILog Log;

        protected AbstractRegistry()
        {
            Log = LogManager.GetLogger(GetType());
            retryTask = new Timer(Retry);
            StartRetryTask();
        }

        private void StartRetryTask()
        {
            Log.Debug("Start retry task");
            retryTask.Change(30000, Timeout.Infinite);
        }

        private void Retry(object instance)
        {
            try
            {
                if (!Monitor.TryEnter(this))
                {
                    return;
                }
                if (failRegisterConfigs.Count <= 0 && failSubscribeConfigs.Count <= 0)
                {
                    StartRetryTask();
                    return;
                }

                var tasks = new List<Task>(failRegisterConfigs.Count + failSubscribeConfigs.Count);
                foreach (var registerConfig in failRegisterConfigs)
                {
                    tasks.Add(Register(registerConfig).ContinueWith(t =>
                    {
                        if (t.IsFaulted || t.Exception != null)
                        {
                            Log.Warn($"Retry register {registerConfig} fail.", t.Exception);
                            return;
                        }
                        failRegisterConfigs.Remove(registerConfig);
                    }));
                }

                foreach (var failSubscribeConfig in failSubscribeConfigs)
                {
                    var config = failSubscribeConfig.Key;
                    var actionArray = failSubscribeConfig.Value.ToArray();
                    var subTasks = new Task[actionArray.Length];
                    for (var i = 0; i < subTasks.Length; i++)
                    {
                        var action = actionArray[i];
                        subTasks[i] = Subscribe(config, action).ContinueWith(t =>
                           {
                               if (t.IsFaulted || t.Exception != null)
                               {
                                   Log.Error($"Retry subscribe {config} fail.", t.Exception);
                                   return;
                               }
                               failSubscribeConfig.Value.Remove(action);
                           });
                    }

                    tasks.Add(Task.WhenAll(subTasks).ContinueWith(t =>
                    {
                        if (failSubscribeConfig.Value.Count <= 0)
                        {
                            failSubscribeConfigs.TryRemove(config, out _);
                        }
                    }));
                }

                Task.WhenAll(tasks).ContinueWith(t => { StartRetryTask(); });
            }
            finally
            {
                if (Monitor.IsEntered(this))
                {
                    Monitor.Exit(this);
                }
            }
        }

        protected abstract Task DoRegister(ServiceConfig serviceInfo);

        protected abstract Task DoSubscribe(ServiceConfig serviceInfo, Action<IList<ServiceConfig>> onChange);

        protected virtual Task Recover()
        {
            foreach (var config in successRegisterConfigs)
            {
                failRegisterConfigs.Add(config);
                successRegisterConfigs.Remove(config);
            }
            foreach (var config in successSubscribeConfigs)
            {
                failSubscribeConfigs.AddOrUpdate(config.Key, config.Value, ((key, old) =>
                {
                    foreach (var action in config.Value)
                    {
                        old.Add(action);
                    }
                    return old;
                }));
                successSubscribeConfigs.TryRemove(config.Key, out _);
            }

            retryTask.Change(0, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public async Task Register(ServiceConfig config)
        {
            try
            {
                await DoRegister(config);
                successRegisterConfigs.Add(config);
            }
            catch (Exception ex)
            {
                failRegisterConfigs.Add(config);
                Log.Warn($"Register {config} fail.", ex);
            }

        }

        public async Task Subscribe(ServiceConfig serviceInfo, Action<IList<ServiceConfig>> onChange)
        {
            try
            {
                await DoSubscribe(serviceInfo, onChange);
                var actions =
                    successSubscribeConfigs.GetOrAdd(serviceInfo, new ConcurrentSet<Action<IList<ServiceConfig>>>());
                actions.Add(onChange);
            }
            catch (Exception e)
            {
                var failActions = successSubscribeConfigs.GetOrAdd(serviceInfo, new ConcurrentSet<Action<IList<ServiceConfig>>>());
                failActions.Add(onChange);
                Log.Warn($"Subscribe {serviceInfo} fail.", e);
            }
        }
    }
}