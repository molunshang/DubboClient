using System;
using System.IO;
using System.Text;

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
#pragma warning disable CS0675 // 对进行了带符号扩展的操作数使用了按位或运算符
                case 5:
                    return (long)stream.ReadByte() << 32 | (long)stream.ReadByte() << 24 | (long)stream.ReadByte() << 16 | (long)stream.ReadByte() << 8 | stream.ReadByte();
                case 6:
                    return (long)stream.ReadByte() << 40 | (long)stream.ReadByte() << 32 | (long)stream.ReadByte() << 24 | (long)stream.ReadByte() << 16 | (long)stream.ReadByte() << 8 | stream.ReadByte();
                case 7:
                    return (long)stream.ReadByte() << 48 | (long)stream.ReadByte() << 40 | (long)stream.ReadByte() << 32 | (long)stream.ReadByte() << 24 | (long)stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte();
                case 8:
                    return (long)stream.ReadByte() << 56 | (long)stream.ReadByte() << 48 | (long)stream.ReadByte() << 40 | (long)stream.ReadByte() << 32 | (long)stream.ReadByte() << 24 | (long)stream.ReadByte() << 16 | (long)stream.ReadByte() << 8 | stream.ReadByte();
#pragma warning restore CS0675 // 对进行了带符号扩展的操作数使用了按位或运算符
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static double ReadDouble(this Stream stream)
        {
            return BitConverter.Int64BitsToDouble(ReadLong(stream));
        }

        public static void ReadBytes(this Stream steam, byte[] buffer, int count = -1)
        {
            if (count <= 0)
            {
                count = buffer.Length;
            }
            var offset = 0;
            while (offset < count)
            {
                offset += steam.Read(buffer, offset, count - offset);
            }
        }

        public static string ReadUtf8String(this Stream stream, int count)
        {
            var str = new StringBuilder();
            while (count > 0)
            {
                var ch = stream.ReadByte();
                if (ch < 0x80)
                {
                    str.Append((char)ch);
                }
                else if ((ch & 0xe0) == 0xc0)
                {

                    str.Append((char)(((ch & 0x1f) << 6) + (stream.ReadByte() & 0x3f)));
                }
                else if ((ch & 0xf0) == 0xe0)
                {

                    str.Append((char)(((ch & 0x0f) << 12) + ((stream.ReadByte() & 0x3f) << 6) + (stream.ReadByte() & 0x3f)));
                }
                else
                    throw new ArgumentException($"bad utf-8 encoding at {(char)ch}");

                count--;
            }
            return str.ToString();
        }
    }
}
