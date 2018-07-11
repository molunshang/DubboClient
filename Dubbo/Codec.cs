using System;
using System.IO;
using System.Net.Sockets;

namespace Dubbo
{
    public abstract class Codec
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
        protected abstract void EncodeBody(Stream writer);
        public void Encode(Request request)
        {
            var header = new byte[HeaderLength];
            header.WriteUShort(Magic);
            header[2] = RequestFlag | 2;
            header[2] |= TwowayFlag;
            header.WriteLong(request.RequestId, 4);
            // ArrayPool<byte> pool;
        }

        public void Decode()
        {
            // hessiancsharp.io.CHessianOutput out=new hessiancsharp.io.CHessianOutput();
        }
    }
}
