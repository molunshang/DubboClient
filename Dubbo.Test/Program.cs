using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Dubbo.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!"); 
            char c = (char)48;
            var longNum = new byte[8];
            longNum.WriteLong(12345);
            var requestId = longNum.ReadLong(); ;
            var client = new TcpClient();
            client.Connect(IPAddress.Parse("10.255.72.55"), 20880);
            var codec = new Codec();
            var request = new Request()
            {
                RequestId = requestId,
                MethodName = "appetite",
                ParameterTypeInfo = "Ljava/lang/String;",
                Arguments = new[] { "057489D2-3FDD-4EFB-A2F0-91C337C3D3DC" },
                Attachments = new Dictionary<string, string>()
            };
            request.Attachments["path"] = "com.fengjr.usercenter.api.RiskAppetiteService";
            request.Attachments["interface"] = "com.fengjr.usercenter.api.RiskAppetiteService";
            request.Attachments["version"] = "1.0.0";
            request.Attachments["timeout"] = "100000";
            var channel = client.GetStream();
            codec.Encode(request, channel);
            channel.Flush();
            var resHeader = new byte[16];
            var size = channel.Read(resHeader, 0, 16);
            while (size < 16)
            {
                size += channel.Read(resHeader, 0, 16 - size);
            }
            System.Console.WriteLine(resHeader.ReadLong(4));
            var bodyLength = resHeader.ReadInt(12);
            var body = new byte[bodyLength];
            size = channel.Read(body, 0, bodyLength);
            while (size < bodyLength)
            {
                size += channel.Read(body, size, bodyLength - size);
            }
            var stream = new MemoryStream(body);
            System.Console.WriteLine(stream.Position);
            var input = new Hessian2Input(stream);
            System.Console.WriteLine(stream.Position);
            stream.Seek(0, SeekOrigin.Begin);
            System.Console.WriteLine(stream.Position);
            System.Console.WriteLine("status:{0}", input.ReadString());
            // var obj = input.ReadObject();
            // System.Console.WriteLine(obj);
        }
    }
}
