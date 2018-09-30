using System;
using System.IO;

namespace Hessian.Lite
{
    public static class StreamUtils
    {
        public static void WriteLong(this Stream stream, long val)
        {
            stream.WriteByte((byte)(val >> 56));
            stream.WriteByte((byte)(val >> 48));
            stream.WriteByte((byte)(val >> 40));
            stream.WriteByte((byte)(val >> 32));
            stream.WriteByte((byte)(val >> 24));
            stream.WriteByte((byte)(val >> 16));
            stream.WriteByte((byte)(val >> 8));
            stream.WriteByte((byte)val);
        }

        public static void WriteInt(this Stream stream, int val)
        {
            stream.WriteByte((byte)(val >> 24));
            stream.WriteByte((byte)(val >> 16));
            stream.WriteByte((byte)(val >> 8));
            stream.WriteByte((byte)val);
        }

        public static void WriteInt(this Stream stream, long val)
        {
            stream.WriteByte((byte)(val >> 24));
            stream.WriteByte((byte)(val >> 16));
            stream.WriteByte((byte)(val >> 8));
            stream.WriteByte((byte)val);
        }

        public static void WriteUtf8String(this Stream stream, string str)
        {
            WriteUtf8String(stream, str, 0, str.Length);
        }

        public static void WriteUtf8String(this Stream stream, string str, int offset, int length)
        {
            for (var end = offset + length; offset < end; offset++)
            {
                var ch = str[offset];
                if (ch < 0x80)
                {
                    stream.WriteByte((byte)ch);
                }
                else if (ch < 0x800)
                {
                    stream.WriteByte((byte)(0xc0 + ((ch >> 6) & 0x1f)));
                    stream.WriteByte((byte)(0x80 + (ch & 0x3f)));
                }
                else
                {
                    stream.WriteByte((byte)(0xe0 + ((ch >> 12) & 0xf)));
                    stream.WriteByte((byte)(0x80 + ((ch >> 6) & 0x3f)));
                    stream.WriteByte((byte)(0x80 + (ch & 0x3f)));
                }
            }
        }

        public static int ReadInt(this Stream stream, int bits = 4)
        {
            switch (bits)
            {
                case 1:
                    return stream.ReadByte();
                case 2:
                    return stream.ReadByte() << 8 | stream.ReadByte();
                case 3:
                    return stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte();
                case 4:
                    return stream.ReadByte() << 24 | stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static long ReadLong(this Stream stream, int bits = 8)
        {
            switch (bits)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    return ReadInt(stream, bits);
                case 5:
                    return ReadInt(stream, bits) << 8 | stream.ReadByte();
                case 6:
                    return ReadInt(stream, bits) << 16 | stream.ReadByte() << 8 | stream.ReadByte();
                case 7:
                    return ReadInt(stream, bits) << 24 | stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte();
                case 8:
                    return ReadInt(stream, bits) << 32 | stream.ReadByte() << 24 | stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static double ReadDouble(this Stream stream)
        {
            return BitConverter.Int64BitsToDouble(ReadLong(stream));
        }
    }
}
