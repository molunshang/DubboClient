using System;
using System.IO;

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
            switch (tag)
            {
                case Constants.True:
                    return true;
                case Constants.False:
                    return false;

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
                    return tag != Constants.IntOneByte;

                // INT_BYTE = 0
                case 0xc8:
                    return _reader.ReadByte() != 0;

                // INT_BYTE != 0
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    _reader.ReadByte();
                    return true;

                // INT_SHORT = 0
                case 0xd4:
                    return _reader.ReadInt(2) != 0;

                // INT_SHORT != 0
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    _reader.ReadInt(2);
                    return true;

                case Constants.Int:
                    return _reader.ReadInt() != 0;

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
                    return tag != Constants.LongOneByte;

                // LONG_BYTE = 0
                case 0xf8:
                    return _reader.ReadByte() != 0;

                // LONG_BYTE != 0
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    _reader.ReadByte();
                    return true;

                // INT_SHORT = 0
                case 0x3c:
                    return _reader.ReadInt(2) != 0;

                // INT_SHORT != 0
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    _reader.ReadInt(2);
                    return true;

                case Constants.LongFourByte:
                    return _reader.ReadLong(4) != 0;

                case Constants.Long:
                    return _reader.ReadLong() != 0;

                case Constants.DoubleZero:
                    return false;

                case Constants.DoubleOne:
                    return true;

                case Constants.DoubleByte:
                    return _reader.ReadByte() != 0;

                case Constants.DoubleShort:
                    return _reader.ReadInt(2) != 0;

                case Constants.DoubleInt:
                    return _reader.ReadInt() != 0;

                case Constants.Double:
                    return _reader.ReadDouble() != 0.0;

                case Constants.Null:
                    return false;

                default:
                    throw RaiseError("bool", tag);
            }
        }


        public short ReadShort()
        {
            return (short)ReadInt();
        }

        public int ReadInt()
        {
            var tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.Null:
                    return 0;

                case Constants.False:
                    return 0;

                case Constants.True:
                    return 1;

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
                    return tag - Constants.IntOneByte;

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
                    return ((tag - Constants.IntTwoByte) << 8) | _reader.ReadByte();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return ((tag - Constants.IntThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();

                case Constants.Int:
                case Constants.LongFourByte:
                    return _reader.ReadInt();

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
                    return tag - Constants.LongOneByte;

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
                    return ((tag - Constants.LongTwoByte) << 8) + _reader.ReadByte();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return ((tag - Constants.LongThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();

                case Constants.Long:
                    return (int)_reader.ReadLong();

                case Constants.DoubleZero:
                    return 0;

                case Constants.DoubleOne:
                    return 1;

                case Constants.DoubleByte:
                    return _reader.ReadByte();

                case Constants.DoubleShort:
                    return (short)(_reader.ReadByte() << 8 | _reader.ReadByte());

                case Constants.DoubleInt:
                    {
                        return (int)(0.001 * _reader.ReadInt());
                    }

                case Constants.Double:
                    return (int)_reader.ReadDouble();

                default:
                    throw RaiseError("int", tag);
            }
        }

        public long ReadLong()
        {
            int tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.Null:
                    return 0;

                case Constants.False:
                    return 0;

                case Constants.True:
                    return 1;

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
                    return tag - Constants.IntOneByte;

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
                    return ((tag - Constants.IntTwoByte) << 8) | _reader.ReadByte();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return ((tag - Constants.IntThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();

                case Constants.DoubleByte:
                    return (byte)_reader.ReadByte();

                case Constants.DoubleShort:
                    return (short)(_reader.ReadByte() << 8 | _reader.ReadByte());

                case Constants.Int:
                case Constants.LongFourByte:
                    return _reader.ReadInt();

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
                    return tag - Constants.LongOneByte;

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
                    return ((tag - Constants.LongTwoByte) << 8) | _reader.ReadByte();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return ((tag - Constants.LongThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();

                case Constants.Long:
                    return _reader.ReadLong();

                case Constants.DoubleZero:
                    return 0;

                case Constants.DoubleOne:
                    return 1;

                case Constants.DoubleInt:
                    {
                        return (long)(0.001 * _reader.ReadInt());
                    }

                case Constants.Double:
                    return (long)_reader.ReadDouble();

                default:
                    throw RaiseError("long", tag);
            }
        }

        public float ReadFloat()
        {
            return (float)ReadDouble();
        }

        public double ReadDouble()
        {
            int tag = _reader.ReadByte();
            switch (tag)
            {
                case Constants.Null:
                    return 0;

                case Constants.False:
                    return 0;

                case Constants.True:
                    return 1;
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
                    return tag - 0x90;

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
                    return ((tag - Constants.IntTwoByte) << 8) | _reader.ReadByte();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return ((tag - Constants.IntThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();

                case Constants.Int:
                case Constants.LongFourByte:
                    return _reader.ReadInt();

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
                    return tag - Constants.LongOneByte;

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
                    return (tag - Constants.LongTwoByte) << 8 | _reader.ReadByte();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return ((tag - Constants.LongThreeByte) << 16) | _reader.ReadByte() << 8 | _reader.ReadByte();

                case Constants.Long:
                    return _reader.ReadLong();

                case Constants.DoubleZero:
                    return 0;

                case Constants.DoubleOne:
                    return 1;

                case Constants.DoubleByte:
                    return (byte)_reader.ReadByte();

                case Constants.DoubleShort:
                    return _reader.ReadInt(2);

                case Constants.DoubleInt:
                    return 0.001 * _reader.ReadInt();
                case Constants.Double:
                    return _reader.ReadDouble();

                default:
                    throw RaiseError("double", tag);
            }
        }

        public DateTime ReadDate()
        {
            int tag = _reader.ReadByte();
            if (tag == Constants.DateTimeMillisecond)
            {
                return DateTimeUtils.UtcStartTime.AddMilliseconds(_reader.ReadLong());
            }
            if (tag == Constants.DateTimeMinute)
            {
                return DateTimeUtils.UtcStartTime.AddMinutes(_reader.ReadInt());
            }
            throw RaiseError("date", tag);
        }

        private int readState;
        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            if (readState == 2)
            {
                readState = 0;
                return -1;
            }
            else if (readState == 0)
            {
                var tag = _reader.ReadByte();
                switch (tag)
                {
                    case Constants.Null:
                        return -1;
                    case Constants.BinaryChunk:
                    case Constants.BinaryFinalChunk:
                        break;
                    default:
                        throw new Exception();
                }
            }

            return -1;
            //else if (_chunkLength == 0)
            //{
            //    int tag = read();

            //    switch (tag)
            //    {
            //        case 'N':
            //            return -1;

            //        case 'B':
            //        case BC_BINARY_CHUNK:
            //            _isLastChunk = tag == 'B';
            //            _chunkLength = (read() << 8) + read();
            //            break;

            //        default:
            //            throw expect("binary", tag);
            //    }
            //}

            //while (length > 0)
            //{
            //    if (_chunkLength > 0)
            //    {
            //        buffer[offset++] = (byte)read();
            //        _chunkLength--;
            //        length--;
            //        readLength++;
            //    }
            //    else if (_isLastChunk)
            //    {
            //        if (readLength == 0)
            //            return -1;
            //        else
            //        {
            //            _chunkLength = END_OF_DATA;
            //            return readLength;
            //        }
            //    }
            //    else
            //    {
            //        int tag = read();

            //        switch (tag)
            //        {
            //            case 'B':
            //            case BC_BINARY_CHUNK:
            //                _isLastChunk = tag == 'B';
            //                _chunkLength = (read() << 8) + read();
            //                break;

            //            default:
            //                throw expect("binary", tag);
            //        }
            //    }
            //}

            //if (readLength == 0)
            //    return -1;
            //else if (_chunkLength > 0 || !_isLastChunk)
            //    return readLength;
            //else
            //{
            //    _chunkLength = END_OF_DATA;
            //    return readLength;
            //}
        }
    }
}
