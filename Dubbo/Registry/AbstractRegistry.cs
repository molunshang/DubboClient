using Dubbo.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Dubbo.Registry
{
    public abstract class AbstractRegistry
    {
        protected ILog log;

        protected AbstractRegistry()
        {
            log = LogManager.GetLogger(GetType());
        }

        protected abstract Task DoRegister(ServiceConfig serviceInfo);

        protected abstract Task DoSubscribe(ServiceConfig serviceInfo, Action<IList<ServiceConfig>> onChange);

        public void Register(ServiceConfig serviceInfo)
        {
            DoRegister(serviceInfo).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    log.Error("register fail.", t.Exception);
                }
            });
        }

        public void Subscribe(ServiceConfig serviceInfo, Action<IList<ServiceConfig>> onChange)
        {
            DoSubscribe(serviceInfo, onChange).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    log.Error("subscribe fail.", t.Exception);
                }
            });
        }
    }
}