using System;

namespace Dubbo.Utils
{
    public static class ByteUtils
    {
        public static void WriteUShort(this byte[] buffer, ushort num, int offset = 0)
        {
            buffer[offset] = (byte)(num >> 8);
            buffer[offset + 1] = (byte)(num);
        }

        public static void WriteInt(this byte[] buffer, int num, int offset = 0)
        {
            buffer[offset] = (byte)(num >> 24);
            buffer[offset + 1] = (byte)(num >> 16);
            buffer[offset + 2] = (byte)(num >> 8);
            buffer[offset + 3] = (byte)(num);
        }

        public static int ReadInt(this byte[] buffer, int offset = 0)
        {
            return buffer[offset] << 24 | buffer[offset + 1] << 16 | buffer[offset + 2] << 8 | buffer[offset + 3];
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

        public static long ReadLong(this byte[] buffer, int offset = 0)
        {
            return (long)buffer[offset] << 56 | (long)buffer[offset + 1] << 48 | (long)buffer[offset + 2] << 40 | (long)buffer[offset + 3] << 32 | (long)buffer[offset + 4] << 24 | (long)buffer[offset + 5] << 16 | (long)buffer[offset + 6] << 8 | (long)buffer[offset + 7];
        }

        public static void WriteBytes(this byte[] buffer, byte[] data, int offset = 0)
        {
            Array.Copy(data, 0, buffer, offset, data.Length);
        }
    }
}
