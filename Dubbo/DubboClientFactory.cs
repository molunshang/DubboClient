using Dubbo.Attribute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dubbo
{
    public class DubboClientFactory
    {
        private readonly ConcurrentDictionary<Type, object> _singleObjects = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, string> _typeMaps = new ConcurrentDictionary<Type, string>();

        private static readonly IEqualityComparer<MethodInfo> MethodEqualityComparer =
            Collection.EqualityComparer.CreateEqualityComparer<MethodInfo>(
                (x, y) =>
                {
                    if (x.Name != y.Name || x.ReflectedType != y.ReturnType)
                    {
                        return false;
                    }

                    ParameterInfo[] xParameters = x.GetParameters(), yParameterInfos = y.GetParameters();
                    if (xParameters.Length != yParameterInfos.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < xParameters.Length; i++)
                    {
                        Type xInfo = xParameters[i].ParameterType, yInfo = yParameterInfos[i].ParameterType;
                        if (xInfo.IsGenericType && yInfo.IsGenericType)
                        {
                            if (xInfo.GetGenericTypeDefinition() != yInfo.GetGenericTypeDefinition())
                            {
                                return false;
                            }
                        }
                        else if (xInfo.IsGenericType || yInfo.IsGenericType)
                        {
                            return false;
                        }
                        else if (xInfo != yInfo)
                        {
                            return false;
                        }
                    }

                    return true;
                }, m => $"{m.ReflectedType.FullName}.{m.Name}".GetHashCode());

        public DubboClientFactory()
        {
            _typeMaps.TryAdd(typeof(void), "V");
            _typeMaps.TryAdd(typeof(bool), "Z");
            _typeMaps.TryAdd(typeof(char), "C");
            _typeMaps.TryAdd(typeof(byte), "B");
            _typeMaps.TryAdd(typeof(short), "S");
            _typeMaps.TryAdd(typeof(ushort), "I");
            _typeMaps.TryAdd(typeof(int), "I");
            _typeMaps.TryAdd(typeof(uint), "J");
            _typeMaps.TryAdd(typeof(long), "J");
            _typeMaps.TryAdd(typeof(double), "D");
            _typeMaps.TryAdd(typeof(float), "F");
            _typeMaps.TryAdd(typeof(decimal), "Ljava/math/BigDecimal");
            _typeMaps.TryAdd(typeof(decimal?), "Ljava/math/BigDecimal");
            _typeMaps.TryAdd(typeof(DateTime), "Ljava/util/Date");
            _typeMaps.TryAdd(typeof(DateTime?), "Ljava/util/Date");

            _typeMaps.TryAdd(typeof(object), "Ljava/lang/Object");
            _typeMaps.TryAdd(typeof(int?), "Ljava/lang/Integer");
            _typeMaps.TryAdd(typeof(long?), "Ljava/lang/Long");
            _typeMaps.TryAdd(typeof(byte?), "Ljava/lang/Byte");
            _typeMaps.TryAdd(typeof(uint?), "Ljava/lang/Long");
            _typeMaps.TryAdd(typeof(ushort?), "Ljava/lang/Integer");

            _typeMaps.TryAdd(typeof(IDictionary<,>), "Ljava/util/Map");
            _typeMaps.TryAdd(typeof(Dictionary<,>), "Ljava/util/HashMap");
            _typeMaps.TryAdd(typeof(IList<>), "Ljava/util/List");
            _typeMaps.TryAdd(typeof(List<>), "Ljava/util/ArrayList");
        }

        private static DefaultProxy Convert(object instance)
        {
            return (DefaultProxy)instance;
        }

        private string GetTypeDesc(ParameterInfo[] parameters)
        {
            if (parameters == null || parameters.Length <= 0)
            {
                return string.Empty;
            }

            var typeInfo = new StringBuilder();
            foreach (var parameter in parameters)
            {
                var typeAttribute = parameter.GetCustomAttribute<DubboTypeAttribute>();
                if (typeAttribute != null)
                {
                    typeInfo.Append('L');
                    typeInfo.Append(typeAttribute.TargetType);
                    typeInfo.Append(';');
                }
                else
                {
                    var type = parameter.ParameterType;
                    if (type.IsGenericType)
                    {
                        type = type.GetGenericTypeDefinition();
                    }
                    if (!_typeMaps.TryGetValue(type, out var desc))
                    {
                        var info = new StringBuilder();
                        while (type.IsArray)
                        {
                            info.Append('[');
                            type = type.GetElementType();
                        }
                        info.Append('L');
                        typeAttribute = type.GetCustomAttribute<DubboTypeAttribute>();
                        info.Append(typeAttribute != null ? typeAttribute.TargetType : type.FullName.Replace('.', '/'));
                        info.Append(';');
                        desc = info.ToString();
                        _typeMaps.TryAdd(type, desc);
                    }
                    typeInfo.Append(desc);
                }
            }

            return typeInfo.ToString();
        }

        public void RegisterTypeMap(Type type, string targetType)
        {
            _typeMaps.TryAdd(type, targetType);
        }

        public T RequireClient<T>()
        {
            var type = typeof(T);
            if (_singleObjects.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }

            lock (_singleObjects)
            {
                if (_singleObjects.TryGetValue(type, out instance))
                {
                    return (T)instance;
                }
                var service = type.GetCustomAttribute<DubboServiceAttribute>();
                if (service == null)
                {
                    throw new ArgumentException($"the type {type.Name} is not a dubbo service !");
                }
                var client = DispatchProxy.Create<T, DefaultProxy>();
                var proxy = Convert(client);
                proxy.MethodDictionary = new Dictionary<MethodInfo, InvokeContext>(MethodEqualityComparer);
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var methodInfo in methods)
                {
                    var dubboMethod = methodInfo.GetCustomAttribute<DubboMethodAttribute>();
                    if (dubboMethod == null)
                    {
                        continue;
                    }
                    var parameterDesc = GetTypeDesc(methodInfo.GetParameters());
                    proxy.MethodDictionary.Add(methodInfo, new InvokeContext
                    {
                        Service = service.TargetService,
                        Group = service.Group,
                        Version = service.Version,
                        Timeout = Math.Max(dubboMethod.TimeOut, service.TimeOut),
                        Method = dubboMethod.TargetMethod,
                        ParameterTypeInfo = parameterDesc
                    });
                }

                _singleObjects.TryAdd(type, proxy);
                return client;
            }
        }

        class DefaultProxy : DispatchProxy
        {
            internal IDictionary<MethodInfo, InvokeContext> MethodDictionary;
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                var name = targetMethod.Name;
                switch (name)
                {
                    case "ToString":
                        return ToString();
                    case "GetHashCode":
                        return GetHashCode();
                    case "Equals":
                        return Equals(args[0]);
                    case "GetType":
                        return GetType();
                    case "MemberwiseClone":
                        return MemberwiseClone();
                    default:
                        if (!MethodDictionary.TryGetValue(targetMethod, out var context))
                        {
                            throw new InvalidOperationException($"unknow method {targetMethod.Name}");
                        }
                        var request = new Request() { IsTwoWay = true, MethodName = context.Method, Arguments = args, Service = context.Service, ParameterTypeInfo = context.ParameterTypeInfo };
                        break;

                }
                //var dubboMethod = targetMethod.GetCustomAttribute<DubboMethodAttribute>();
                //var method = dubboMethod == null ? targetMethod.Name.Substring(0, 1).ToLower() + targetMethod.Name.Substring(1, targetMethod.Name.Length - 1) : dubboMethod.TargetMethod;
                //
                throw new NotImplementedException();
            }
        }
    }
}
