using Dubbo.Utils;
using System;

namespace Dubbo.Remote
{
    public class Header
    {
        public const int HeaderLength = 16;
        public const ushort Magic = 0xdabb;
        public const byte MagicFirst = 218;
        public const byte MagicSecond = 187;
        public const byte RequestFlag = 128;
        public const byte TwowayFlag = 64;
        public const byte EventFlag = 32;
        public const int HessianSerialize = 2;
        public long RequestId { get; set; }
        public byte Status { get; set; }
        public bool IsRequest { get; set; }
        public bool IsEvent { get; set; }
        public bool IsTwoWay { get; set; }
        public int BodyLength { get; set; }
        public Header()
        {

        }

        public static Header Parse(byte[] headerBytes)
        {
            if (headerBytes == null || headerBytes.Length < HeaderLength)
            {
                throw new ArgumentException($"the headerBytes is null or length less then {HeaderLength.ToString()}");
            }
            var header = new Header
            {
                RequestId = headerBytes.ReadLong(4),
                Status = headerBytes[3],
                IsRequest = (headerBytes[2] & RequestFlag) != 0,
                IsEvent = (headerBytes[2] & EventFlag) != 0,
                IsTwoWay = (headerBytes[2] & TwowayFlag) != 0,
                BodyLength = headerBytes.ReadInt(12)
            };
            return header;
        }
    }
}