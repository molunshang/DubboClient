using System;
using System.IO;
using System.Net.Sockets;
using Hessian.IO;

namespace Dubbo
{
    public class Codec
    {
        const int HeaderLength = 16;
        const ushort Magic = 0xdabb;
        const byte MagicFirst = 218;
        const byte MagicSecond = 187;
        const byte RequestFlag = 128;
        const byte TwowayFlag = 64;
        const byte EventFlag = 32;
        const int SerializationMask = 31;
        // protected static final short    MAGIC              = (short) 0xdabb;
        // protected static final byte     MAGIC_HIGH         = Bytes.short2bytes(MAGIC)[0];
        // protected static final byte     MAGIC_LOW          = Bytes.short2bytes(MAGIC)[1];

        public void Encode(Request request, Stream outputStream)
        {
            var header = new byte[HeaderLength];
            header.WriteUShort(Magic);
            header[2] = RequestFlag | 2;
            header[2] = (byte)(header[2] | TwowayFlag);
            header.WriteLong(request.RequestId, 4);
            using (var dataStream = new PoolMemoryStream())
            {
                var output = new Hessian2Output(dataStream);
                output.WriteString("2.0.0");
                output.WriteString(request.Attachments["path"]);
                output.WriteString(request.Attachments["version"]);
                output.WriteString(request.MethodName);
                output.WriteString(request.ParameterTypeInfo);
                if (request.Arguments != null && request.Arguments.Length > 0)
                {
                    foreach (var arg in request.Arguments)
                    {
                        output.Write(arg);
                    }
                }
                output.WriteObject(request.Attachments);
                header.WriteInt((int)dataStream.Length, 12);
                outputStream.Write(header, 0, header.Length);
                dataStream.CopyTo(outputStream);
            }
        }

        public void Decode()
        {
            // hessiancsharp.io.CHessianOutput out=new hessiancsharp.io.CHessianOutput();
        }
    }
}
