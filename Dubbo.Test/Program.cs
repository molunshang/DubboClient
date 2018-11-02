using Dubbo.Attribute;
using Dubbo.Config;
using Dubbo.Registry;
using Hessian.Lite.Util;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Reflection;

namespace Dubbo.Test
{
    class Program
    {
        static void TestDubboInvoke()
        {
            //var request = new Request
            //{
            //    MethodName = "getLoanById",
            //    ParameterTypeInfo = "Ljava/lang/String;",
            //    Arguments = new[] { "20180328_86F1_406E_B86B_EE6F0D09D98E" },
            //    Attachments = new Dictionary<string, string>()
            //};
            //request.Attachments["path"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            //request.Attachments["interface"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            ////            request.Attachments["version"] = "1.0.0";
            //request.Attachments["timeout"] = "100000";

            //var client = new TcpClient();
            //client.Connect(IPAddress.Parse("10.254.21.59"), 20880);

            //var channel = client.GetStream();
            //Codec.EncodeRequest(request, channel);
            //channel.Flush();
            //var resHeader = new byte[16];
            //var size = channel.Read(resHeader, 0, 16);
            //while (size < 16)
            //{
            //    size += channel.Read(resHeader, 0, 16 - size);
            //}

            //Console.WriteLine(resHeader.ReadLong(4));
            //var bodyLength = resHeader.ReadInt(12);
            //var body = new byte[bodyLength];
            //size = channel.Read(body, 0, bodyLength);
            //while (size < bodyLength)
            //{
            //    size += channel.Read(body, size, bodyLength - size);
            //}

            //var stream = new MemoryStream(body);
            //var input = new Hessian2Reader(stream);
            //var flag = input.ReadObject();
            //Console.WriteLine(flag);
        }

        static void InvokeDubbo(ServiceConfig config)
        {
            //var request = new Request
            //{
            //    MethodName = "sayHello",
            //    ParameterTypeInfo = "Ljava/lang/String;",
            //    Arguments = new[] { "invoke from .net client" },
            //    Attachments = new Dictionary<string, string>(),
            //    IsTwoWay = true
            //};
            //request.Attachments["path"] = "org.apache.dubbo.demo.DemoService";
            //request.Attachments["interface"] = "org.apache.dubbo.demo.DemoService";
            ////            request.Attachments["version"] = "1.0.0";
            //request.Attachments["timeout"] = "100000";
            //var connection = new Connection(config.Host, config.Port);
            //connection.Connect().ContinueWith(t =>
            //{
            //    if (t.IsCompletedSuccessfully)
            //    {
            //        connection.Send(request);
            //        return;
            //    }
            //    Console.WriteLine(t.Exception);
            //});
        }

        class TestProxy : DispatchProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                Console.WriteLine("Invoke Method " + targetMethod.Name);
                return targetMethod.ReturnType.Default();
            }
        }

        [DubboService(TargetService = "org.apache.dubbo.demo.DemoService")]
        public interface ITest
        {
            [DubboMethod(TargetMethod = "sayHello")]
            string Hello(string arg);

            void Invoke<T>(T t);
        }

        static void Main(string[] args)
        {

            //            ZooKeeper zookeeper = new ZooKeeper("", 60 * 1000, ZookeeperWatcherWrapper.ProcessChange(((e, self) => { })));
            IZookeeperClient zookeeper = new ZookeeperClient(new ZookeeperClientOptions()
            {
                ConnectionString = "127.0.0.1:2181",
                ConnectionTimeout = TimeSpan.FromSeconds(30)
            });
            var factory = new DubboClientFactory(new ZookeeperRegistry(zookeeper));
            var test = factory.RequireClient<ITest>();
            var res = test.Hello("simple client");
            Console.WriteLine(res);
            //var config = new ServiceConfig()
            //{
            //    Address = "169.254.89.54",
            //    Application = ".net client",
            //    Category = "consumers",
            //    Protocol = ServiceConfig.DubboConsumer,
            //    ServiceName = "org.apache.dubbo.demo.DemoService",
            //    Methods = new[] { "sayHello" },
            //    Side = "consumer"
            //};
            //var register = new ZookeeperRegistry(zookeeper);
            //var providerConfigs = new List<ServiceConfig>();
            //var tasks = new Task[2];
            //tasks[0] = register.Register(config);
            //tasks[1] = register.Subscribe(config, list => { providerConfigs.AddRange(list); });
            //Task.WaitAll(tasks);
            //foreach (var providerConfig in providerConfigs)
            //{
            //    Console.WriteLine(providerConfig);
            //    InvokeDubbo(providerConfig);
            //}
            Console.ReadKey();
        }
    }
}