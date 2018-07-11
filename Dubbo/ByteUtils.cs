using System;
using System.Collections.Generic;

namespace Dubbo
{
    public static class ByteUtils
    {
        public static void WriteUShort(this byte[] buffer, ushort num, int offset = 0)
        {
            buffer[offset] = (byte)(num >> 8);
            buffer[offset + 1] = (byte)(num);
        }

        public static void WriteLong(this byte[] buffer, long num, int offset = 0)
        {
            buffer[offset] = (byte)(num >> 56);
            buffer[offset + 1] = (byte)(num >> 48);
            buffer[offset + 2] = (byte)(num >> 40);
            buffer[offset + 3] = (byte)(num >> 32);
            buffer[offset + 4] = (byte)(num >> 24);
            buffer[offset + 5] = (byte)(num >> 16);
            buffer[offset + 6] = (byte)(num >> 8);
            buffer[offset + 7] = (byte)(num);
        }
    }
}
