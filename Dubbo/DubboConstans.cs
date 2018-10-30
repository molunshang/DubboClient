using Dubbo.Attribute;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Dubbo
{
    public class DubboClientFactory
    {
        private ConcurrentDictionary<Type, object> _singleObjects;
        public T RequireClient<T>()
        {
            var type = typeof(T);
            if (_singleObjects.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }

            var service = type.GetCustomAttribute(typeof(DubboServiceAttribute));
            if (service == null)
            {
                throw new ArgumentException($"the type {type.Name} is not a dubbo service !");
            }

            var result = DispatchProxy.Create<T, DispatchProxy>();
            _singleObjects.TryAdd(type, result);
            return result;
        }

        class DefaultProxy : DispatchProxy
        {
            public DubboServiceAttribute DubboService { get; set; }
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                var dubboMethod = targetMethod.GetCustomAttribute<DubboMethodAttribute>();
                var method = dubboMethod == null ? targetMethod.Name.Substring(0, 1).ToLower() + targetMethod.Name.Substring(1, targetMethod.Name.Length - 1) : dubboMethod.TargetMethod;
                var serviceName = DubboService.TargetService;
                var request = new Request() { IsTwoWay = true, MethodName = method, Arguments = args, ParameterTypeInfo = "" };
                throw new NotImplementedException();
            }
        }
    }
}
