using Dubbo.Config;
using Dubbo.Registry;
using Hessian.Lite;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Hessian.Lite.Util;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;

namespace Dubbo.Test
{
    class Program
    {
        static void TestDubboInvoke()
        {
            var longNum = new byte[8];
            longNum.WriteLong(12345);
            var requestId = longNum.ReadLong();
            ;
            var request = new Request
            {
                RequestId = requestId,
                MethodName = "getLoanById",
                ParameterTypeInfo = "Ljava/lang/String;",
                Arguments = new[] {"20180328_86F1_406E_B86B_EE6F0D09D98E"},
                Attachments = new Dictionary<string, string>()
            };
            request.Attachments["path"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            request.Attachments["interface"] = "com.fengjr.fengchu.dubbo.api.LoanService";
//            request.Attachments["version"] = "1.0.0";
            request.Attachments["timeout"] = "100000";

            var client = new TcpClient();
            client.Connect(IPAddress.Parse("10.254.21.59"), 20880);

            var channel = client.GetStream();
            Codec.EncodeRequest(request, channel);
            channel.Flush();
            var resHeader = new byte[16];
            var size = channel.Read(resHeader, 0, 16);
            while (size < 16)
            {
                size += channel.Read(resHeader, 0, 16 - size);
            }

            Console.WriteLine(resHeader.ReadLong(4));
            var bodyLength = resHeader.ReadInt(12);
            var body = new byte[bodyLength];
            size = channel.Read(body, 0, bodyLength);
            while (size < bodyLength)
            {
                size += channel.Read(body, size, bodyLength - size);
            }

            var stream = new MemoryStream(body);
            var input = new Hessian2Reader(stream);
            var flag = input.ReadObject();
            Console.WriteLine(flag);
        }

        private static int requestId = 0;

        static void InvokeDubbo(ServiceConfig config)
        {
            var address = config.Address.Split(':');
            var request = new Request
            {
                RequestId = Interlocked.Increment(ref requestId),
                MethodName = "sayHello",
                ParameterTypeInfo = "Ljava/lang/String;",
                Arguments = new[] {"invoke from .net client"},
                Attachments = new Dictionary<string, string>(),
                IsTwoWay = true
            };
            request.Attachments["path"] = "org.apache.dubbo.demo.DemoService";
            request.Attachments["interface"] = "org.apache.dubbo.demo.DemoService";
//            request.Attachments["version"] = "1.0.0";
            request.Attachments["timeout"] = "100000";

            var client = new TcpClient();
            client.Connect(address[0], int.Parse(address[1]));

            var channel = client.GetStream();
            while (true)
            {
                try
                {
                    Codec.EncodeRequest(request, channel);
                    channel.Flush();
                    var res = Codec.DecodeResponse(channel, typeof(string));
                    Console.WriteLine(res.IsOk ? res.Result : res.ErrorMessage);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        static void Main(string[] args)
        {
//            ZooKeeper zookeeper = new ZooKeeper("", 60 * 1000, ZookeeperWatcherWrapper.ProcessChange(((e, self) => { })));
            IZookeeperClient zookeeper = new ZookeeperClient(new ZookeeperClientOptions()
            {
                ConnectionString = "127.0.0.1:2181",
                ConnectionTimeout = TimeSpan.FromSeconds(30)
            });
            var config = new ServiceConfig()
            {
                Address = "169.254.89.54",
                Application = ".net client",
                Category = "consumers",
                Protocol = ServiceConfig.DubboConsumer,
                ServiceName = "org.apache.dubbo.demo.DemoService",
                Methods = new[] {"sayHello"},
                Side = "consumer"
            };
            var register = new ZookeeperRegistry(zookeeper);
            register.Register(config);
            register.Subscribe(config, list =>
            {
                foreach (var serviceConfig in list)
                {
                    InvokeDubbo(serviceConfig);
                }
            });
            Console.ReadKey();
        }
    }
}