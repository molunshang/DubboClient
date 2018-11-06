using Dubbo.Attribute;
using Dubbo.Config;
using Dubbo.Registry;
using Dubbo.Remote;
using Dubbo.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dubbo
{
    public class DubboClientFactory
    {
        private static readonly IEqualityComparer<MethodInfo> MethodEqualityComparer =
            Collection.EqualityComparer.CreateEqualityComparer<MethodInfo>(
                (x, y) =>
                {
                    if (x.Name != y.Name)
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
                }, m =>
                {
                    var parameterInfo = string.Join(";",
                        m.GetParameters().Select(p =>
                            (p.ParameterType.IsGenericType
                                ? p.ParameterType.GetGenericTypeDefinition()
                                : p.ParameterType).FullName));
                    return $"{m.Name}[{parameterInfo}]".GetHashCode();
                });

        private readonly ConcurrentDictionary<Type, object> _singleObjects = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, string> _typeMaps = new ConcurrentDictionary<Type, string>();

        private readonly AbstractRegistry registry;
        private readonly ConnectionFactory connectionFactory;

        private void Init()
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
            _typeMaps.TryAdd(typeof(decimal), "Ljava/math/BigDecimal;");
            _typeMaps.TryAdd(typeof(DateTime), "Ljava/util/Date;");

            _typeMaps.TryAdd(typeof(object), "Ljava/lang/Object;");
            _typeMaps.TryAdd(typeof(bool?), "Ljava/lang/Boolean;");
            _typeMaps.TryAdd(typeof(char?), "Ljava/lang/Character;");
            _typeMaps.TryAdd(typeof(byte?), "Ljava/lang/Byte;");
            _typeMaps.TryAdd(typeof(short?), "Ljava/lang/Short;");
            _typeMaps.TryAdd(typeof(ushort?), "Ljava/lang/Integer;");
            _typeMaps.TryAdd(typeof(int?), "Ljava/lang/Integer;");
            _typeMaps.TryAdd(typeof(uint?), "Ljava/lang/Long;");
            _typeMaps.TryAdd(typeof(long?), "Ljava/lang/Long;");
            _typeMaps.TryAdd(typeof(double?), "Ljava/lang/Double;");
            _typeMaps.TryAdd(typeof(float?), "Ljava/lang/Float;");
            _typeMaps.TryAdd(typeof(decimal?), "Ljava/math/BigDecimal;");
            _typeMaps.TryAdd(typeof(string), "Ljava/lang/String;");
            _typeMaps.TryAdd(typeof(DateTime?), "Ljava/util/Date;");

            _typeMaps.TryAdd(typeof(IDictionary<,>), "Ljava/util/Map;");
            _typeMaps.TryAdd(typeof(Dictionary<,>), "Ljava/util/HashMap;");
            _typeMaps.TryAdd(typeof(IList<>), "Ljava/util/List;");
            _typeMaps.TryAdd(typeof(List<>), "Ljava/util/ArrayList;");
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
        public DubboClientFactory(AbstractRegistry registry, ConnectionFactory connectionFactory)
        {
            this.registry = registry;
            this.connectionFactory = connectionFactory;
            Init();
        }

        public void RegisterTypeMap(Type type, string targetType)
        {
            _typeMaps.TryAdd(type, targetType.EndsWith(";") ? targetType : targetType + ";");
        }

        public T CreateClient<T>()
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
                var config = new ServiceConfig()
                {
                    Host = NetUtils.GetLocalHost(),
                    Application = ".net client",
                    Category = "consumers",
                    Protocol = ServiceConfig.DubboConsumer,
                    ServiceName = service.TargetService,
                    Side = "consumer"
                };
                var methodDictionary = new Dictionary<MethodInfo, InvokeContext>(MethodEqualityComparer);
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var methodNames = new List<string>(methods.Length);
                foreach (var methodInfo in methods)
                {
                    var dubboMethod = methodInfo.GetCustomAttribute<DubboMethodAttribute>();
                    if (dubboMethod == null)
                    {
                        continue;
                    }

                    var returnType = methodInfo.ReturnType;
                    var isAsync = typeof(Task).IsAssignableFrom(returnType);
                    if (isAsync && returnType.IsGenericType)
                    {
                        returnType = returnType.GetGenericArguments()[0];
                    }
                    var parameterDesc = GetTypeDesc(methodInfo.GetParameters());
                    methodDictionary.Add(methodInfo, new InvokeContext
                    {
                        Service = service.TargetService,
                        Group = service.Group,
                        Version = service.Version,
                        Timeout = Math.Max(dubboMethod.Timeout, service.Timeout),
                        Method = dubboMethod.TargetMethod,
                        ParameterTypeInfo = parameterDesc,
                        IsAsync = isAsync,
                        ReturnType = returnType
                    });
                    methodNames.Add(dubboMethod.TargetMethod);
                }

                config.Methods = methodNames.ToArray();
                var connections = new List<Connection>();
                registry.Register(config).Wait();
                registry.Subscribe(config, providers =>
                {
                    foreach (var provider in providers)
                    {
                        connections.Add(new Connection(provider.Host, provider.Port));
                    }
                }).Wait();
                var client = DispatchProxyAsync.Create<T, DefaultProxy>();
                var proxy = Convert(client);
                proxy.MethodDictionary = new ReadOnlyDictionary<MethodInfo, InvokeContext>(methodDictionary);
                proxy.Connections = connections;
                _singleObjects.TryAdd(type, proxy);
                return client;
            }
        }

        public class DefaultProxy : DispatchProxyAsync
        {
            internal IReadOnlyDictionary<MethodInfo, InvokeContext> MethodDictionary;
            internal IList<Connection> Connections;

            private Random random = new Random();

            private Task<Response> DoInvoke(MethodInfo targetMethod, object[] args)
            {
                if (!MethodDictionary.TryGetValue(targetMethod, out var context))
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
                var connection = Connections[random.Next(0, Connections.Count)];
                if (!connection.IsConnected)
                {
                    connection.Connect();
                }
                return connection.Send(request);
            }

            public override object Invoke(MethodInfo targetMethod, object[] args)
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
                        var result = DoInvoke(targetMethod, args).ConfigureAwait(false);
                        return result.GetAwaiter().GetResult().Result;
                }
            }

            public override Task InvokeAsync(MethodInfo method, object[] args)
            {
                return DoInvoke(method, args);
            }

            public override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
            {
                var result = await DoInvoke(method, args);
                return (T)result.Result;
            }
        }
    }
}
