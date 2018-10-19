using Hessian.Lite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Dubbo.Test
{
    class Program
    {
        static void TestDubboInvoke()
        {
            var longNum = new byte[8];
            longNum.WriteLong(12345);
            var requestId = longNum.ReadLong(); ;
            var codec = new Codec();
            var request = new Request
            {
                RequestId = requestId,
                MethodName = "getLoanById",
                ParameterTypeInfo = "Ljava/lang/String;",
                Arguments = new[] { "20180328_86F1_406E_B86B_EE6F0D09D98E" },
                Attachments = new Dictionary<string, string>()
            };
            request.Attachments["path"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            request.Attachments["interface"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            request.Attachments["version"] = "1.0.0";
            request.Attachments["timeout"] = "100000";

            var client = new TcpClient();
            client.Connect(IPAddress.Parse("10.254.21.59"), 20880);

            var channel = client.GetStream();
            codec.Encode(request, channel);
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
            var res = input.ReadObject();
            Console.WriteLine(res);
        }

        static void Main(string[] args)
        {

            //ZooKeeper zookeeper = new ZooKeeper("bzk1.fengjr.inc:2181", 60 * 1000, new EmptyWatcher());
            //zookeeper.getChildrenAsync("/dubbo").ContinueWith(t =>
            //{
            //    if (!t.IsCompletedSuccessfully || t.Result.Children == null)
            //    {
            //        Console.WriteLine(t.Exception.ToString());
            //        return;
            //    }
            //    foreach (var path in t.Result.Children)
            //    {
            //        Console.WriteLine(path);
            //    }
            //}).Wait();
            var dicType = typeof(Program);
            Console.WriteLine("{0},{1}", dicType.Name, dicType.FullName);

            var stream = File.OpenRead(@"E:\code\dubbo-client\Dubbo.Test\bin\Debug\netcoreapp2.0\out.test");
            var reader = new Hessian2Reader(stream);
            var obj = (IDictionary)reader.ReadObject();
            Console.WriteLine(obj);
        }
    }
}
