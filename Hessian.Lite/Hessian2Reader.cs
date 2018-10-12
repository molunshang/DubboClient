using System;
using System.IO;
using System.Text;

namespace Hessian.Lite
{
    public class Hessian2Reader
    {
        private readonly Stream _reader;

        public Hessian2Reader(Stream reader)
        {
            _reader = reader;
        }

        private Exception RaiseError(string msg, int tag)
        {
            throw new ArgumentException(msg);
        }

        private bool TryReadBool(int tag, out bool result)
        {
            switch (tag)
            {
                case Constants.True:
                    result = true;
                    break;
                case Constants.False:
                    result = false;
                    break;
                default:
                    result = false;
                    return false;
            }
            return true;
        }

        private bool TryReadInt(int tag, out int result)
        {
            switch (tag)
            {
                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:
                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    result = tag - Constants.IntOneByte;
                    break;
                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    result = ((tag - Constants.IntTwoByte) << 8) | _reader.ReadByte();
                    break;
                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    result = ((tag - Constants.IntThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();
                    break;
                case Constants.Int:
                    result = _reader.ReadInt();
                    break;
                default:
                    result = 0;
                    return false;
            }
            return true;
        }

        private bool TryReadLong(int tag, out long result)
        {
            switch (tag)
            {
                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:
                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    result = tag - Constants.LongOneByte;
                    break;
                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    result = ((tag - Constants.LongTwoByte) << 8) + _reader.ReadByte();
                    break;
                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    result = ((tag - Constants.LongThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();
                    break;
                case Constants.Long:
                    result = _reader.ReadLong();
                    break;
                case Constants.LongFourByte:
                    result = _reader.ReadInt();
                    break;
                default:
                    result = 0;
                    return false;
            }
            return true;
        }

        private bool TryReadDouble(int tag, out double result)
        {
            switch (tag)
            {
                case Constants.DoubleZero:
                    result = 0;
                    break;
                case Constants.DoubleOne:
                    result = 1;
                    break;
                case Constants.DoubleInt:
                    result = 0.001 * _reader.ReadInt();
                    break;
                case Constants.Double:
                    result = _reader.ReadDouble();
                    break;
                default:
                    result = 0.0D;
                    return false;
            }
            return true;
        }

        public object ReadObject()
        {
            return null;
        }

        public T ReadObject<T>()
        {
            return default(T);
        }

        public void ReadNull()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return;
            }
            throw RaiseError("null", tag);
        }
        public bool ReadBool()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return false;
            }
            if (TryReadBool(tag, out var boolVal))
            {
                return boolVal;
            }
            if (TryReadInt(tag, out var intVal))
            {
                return intVal != 0;
            }
            if (TryReadLong(tag, out var longVal))
            {
                return longVal != 0;
            }
            if (TryReadDouble(tag, out var doubleVal))
            {
                return doubleVal == 0.0;
            }
            throw RaiseError("bool", tag);
        }

        public short ReadShort()
        {
            return (short)ReadInt();
        }

        public int ReadInt()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return 0;
            }

            if (TryReadInt(tag, out var intResult))
            {
                return intResult;
            }

            if (TryReadBool(tag, out var boolResult))
            {
                return boolResult ? 1 : 0;
            }

            if (TryReadLong(tag, out var longResult))
            {
                return (int)longResult;
            }
            if (TryReadDouble(tag, out var doubleVal))
            {
                return (int)doubleVal;
            }
            throw RaiseError("int", tag);
        }

        public long ReadLong()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return 0;
            }

            if (TryReadLong(tag, out var longResult))
            {
                return longResult;
            }

            if (TryReadInt(tag, out var intResult))
            {
                return intResult;
            }

            if (TryReadBool(tag, out var boolResult))
            {
                return boolResult ? 1 : 0;
            }

            if (TryReadDouble(tag, out var doubleVal))
            {
                return (long)doubleVal;
            }
            throw RaiseError("long", tag);
        }

        public float ReadFloat()
        {
            return (float)ReadDouble();
        }

        public double ReadDouble()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return 0;
            }

            if (TryReadDouble(tag, out var doubleVal))
            {
                return doubleVal;
            }
            if (TryReadInt(tag, out var intResult))
            {
                return intResult;
            }

            if (TryReadBool(tag, out var boolResult))
            {
                return boolResult ? 1 : 0;
            }

            if (TryReadLong(tag, out var longResult))
            {
                return longResult;
            }

            throw RaiseError("double", tag);
        }

        public DateTime ReadDate()
        {
            var tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.DateTimeMillisecond:
                    return DateTimeUtils.UtcStartTime.AddMilliseconds(_reader.ReadLong());
                case Constants.DateTimeMinute:
                    return DateTimeUtils.UtcStartTime.AddMinutes(_reader.ReadInt());
                default:
                    throw RaiseError("date", tag);
            }
        }

        private bool ReadChunkLength(out int length)
        {
            var tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.BinaryChunk:
                    length = _reader.ReadInt(2);
                    return false;
                case Constants.BinaryFinalChunk:
                    length = _reader.ReadInt(2);
                    return true;
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    length = tag - 0x20;
                    return true;
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                    length = (tag - 0x34) << 8 | _reader.ReadByte();
                    return true;
                default:
                    throw RaiseError("bytes", tag);
            }
        }

        public byte[] ReadBytes()
        {
            int length;
            var tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.Null:
                    return null;
                case Constants.BinaryChunk:
                    length = _reader.ReadInt(2);
                    var buffer = new byte[length];
                    using (var dataStream = new MemoryStream())
                    {
                        var isLastChunk = false;
                        while (!isLastChunk)
                        {
                            _reader.ReadBuffer(buffer, length);
                            dataStream.Write(buffer, 0, length);
                            isLastChunk = ReadChunkLength(out length);
                            if (buffer.Length < length)
                            {
                                buffer = new byte[length];
                            }
                        }
                        return dataStream.ToArray();
                    }
                case Constants.BinaryFinalChunk:
                    length = _reader.ReadInt(2);
                    break;
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    length = tag - 0x20;
                    break;
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                    length = (tag - 0x34) << 8 | _reader.ReadByte();
                    break;
                default:
                    throw RaiseError("bytes", tag);
            }
            var chunk = new byte[length];
            _reader.ReadBuffer(chunk);
            return chunk;
        }

        private bool ReadStringLength(out int length)
        {
            var tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.String:
                    length = _reader.ReadInt(2);
                    return false;
                case Constants.StringFinal:
                    length = _reader.ReadInt(2);
                    return true;
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                    length = tag;
                    return true;
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    length = (tag - Constants.StringMediumStart) << 8 + _reader.ReadByte();
                    return true;
                default:
                    throw RaiseError("string", tag);
            }
        }

        public string ReadString()
        {
            var tag = _reader.ReadByte();
            int length;
            switch (tag)
            {
                case Constants.Null:
                    return null;
                case Constants.String:
                    length = _reader.ReadInt(2);
                    var str = new StringBuilder();
                    var isLast = false;
                    while (!isLast)
                    {
                        str.Append(_reader.ReadUtf8String(length));
                        isLast = ReadStringLength(out length);
                    }

                    return str.ToString();
                case Constants.StringFinal:
                    length = _reader.ReadInt(2);
                    break;
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                    length = tag;
                    break;
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    length = (tag - Constants.StringMediumStart) << 8 + _reader.ReadByte();
                    break;
                default:
                    throw RaiseError("string", tag);
            }

            return _reader.ReadUtf8String(length);
        }
    }
}
