using Dubbo.Config;
using System;
using System.Collections.Generic;

namespace Dubbo.Registry
{
    public abstract class AbstractRegistry
    {

        protected abstract void DoRegister(ServiceConfig serviceInfo);

        protected abstract void DoSubscribe(ServiceConfig serviceInfo, Action<IList<ServiceConfig>> onChange);

        public void Register(ServiceConfig serviceInfo)
        {
            DoRegister(serviceInfo);
        }

        public void Subscribe(ServiceConfig serviceInfo, Action<IList<ServiceConfig>> onChange)
        {
            DoSubscribe(serviceInfo, onChange);
        }
    }
}