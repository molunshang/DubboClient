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
    }
}
