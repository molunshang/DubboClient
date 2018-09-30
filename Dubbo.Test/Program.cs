using Hessian.Lite;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dubbo.Test
{
    class Program
    {
        class EmptyWatcher : Watcher
        {
            public override Task process(WatchedEvent @event)
            {
                return Task.CompletedTask;
            }
        }

        static IEnumerable<int> Test()
        {
            yield return 1;
        }

        static void Main(string[] args)
        {
            //var type = typeof(int[]);
            //Console.WriteLine(type.BaseType);
            //uint n = 123;
            //var strem = new MemoryStream();
            //var hessian = new CHessianOutput(strem);
            //hessian.WriteObject(n);
            //File.WriteAllBytes("hessian.test", strem.ToArray());
            //return;
            //Console.WriteLine("Hello World!");
            //var longNum = new byte[8];
            //longNum.WriteLong(12345);
            //var requestId = longNum.ReadLong(); ;
            //var client = new TcpClient();
            //client.Connect(IPAddress.Parse("10.254.21.56"), 20880);
            //var codec = new Codec();
            //var request = new Request
            //{
            //    RequestId = requestId,
            //    MethodName = "getLoanById",
            //    ParameterTypeInfo = "Ljava/lang/String;",
            //    Arguments = new[] { "057489D2-3FDD-4EFB-A2F0-91C337C3D3DC" },
            //    Attachments = new Hashtable()
            //};
            //request.Attachments["path"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            //request.Attachments["interface"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            //request.Attachments["version"] = "1.0.0";
            //request.Attachments["timeout"] = "100000";
            //var channel = client.GetStream();
            //codec.Encode(request, channel);
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
            //File.WriteAllBytes("test.data", body);
            //var stream = new MemoryStream(body);
            //Console.WriteLine(stream.Position);
            //var input = new Hessian2Input(stream);
            //Console.WriteLine(stream.Position);
            //stream.Seek(0, SeekOrigin.Begin);
            //Console.WriteLine(stream.Position);
            //Console.WriteLine("status:{0}", input.ReadString());
            //var obj = input.ReadObject();
            //Console.WriteLine(obj);
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
            //var data = Test();
            SerializeFactory.RegisterDefaultTypeMap(typeof(SortedDictionary<string, int>), "java.util.TreeMap");

            var dic = new SortedDictionary<string, int> { { "1", 1 }, { "2", 2 }, { "3", 3 } };
            var buffer = new MemoryStream();
            var writer = new Hessian2Writer(buffer);
            writer.WriteObject(new byte[] { 1, 23, 4 });
            File.WriteAllBytes("hessian.test", buffer.ToArray());
        }


    }
}
