using Hessian.Lite.Exception;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hessian.Lite
{
    public class Hessian2Reader
    {
        private readonly Stream _reader;
        private readonly List<string> types;
        private readonly List<object> refs;
        public Hessian2Reader(Stream reader)
        {
            _reader = reader.CanSeek ? reader : new BufferedStream(reader);
            types = new List<string>();
            refs = new List<object>();
        }

        private HessianException RaiseError(string msg, int tag)
        {
            throw new HessianException(tag, $"unknown code {tag} where read {msg}");
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

        private bool TryReadDate(int tag, out DateTime result)
        {
            switch (tag)
            {
                case Constants.DateTimeMillisecond:
                    result = DateTimeUtils.UtcStartTime.AddMilliseconds(_reader.ReadLong());
                    return true;
                case Constants.DateTimeMinute:
                    result = DateTimeUtils.UtcStartTime.AddMinutes(_reader.ReadInt());
                    return true;
                default:
                    result = default(DateTime);
                    return false;
            }
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

        private bool TryReadString(int tag, out string result)
        {
            int length;
            switch (tag)
            {
                case Constants.String:
                    length = _reader.ReadInt(2);
                    var str = new StringBuilder();
                    var isLast = false;
                    while (!isLast)
                    {
                        str.Append(_reader.ReadUtf8String(length));
                        isLast = ReadStringLength(out length);
                    }
                    result = str.ToString();
                    return true;
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
                    result = null;
                    return false;
            }

            result = _reader.ReadUtf8String(length);
            return true;
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
            if (tag == Constants.Null)
            {
                return default(DateTime);
            }
            if (TryReadDate(tag, out var result))
            {
                return result;
            }
            throw RaiseError("date", tag);
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

        private bool TryReadBytes(int tag, out byte[] result)
        {
            int length;
            switch (tag)
            {
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
                        result = dataStream.ToArray();
                    }

                    return true;
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
                    result = null;
                    return false;
            }
            result = new byte[length];
            _reader.ReadBuffer(result);
            return true;
        }
        public byte[] ReadBytes()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return null;
            }

            if (TryReadBytes(tag, out var result))
            {
                return result;
            }
            throw RaiseError("bytes", tag);
        }



        public string ReadString()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return null;
            }

            if (TryReadString(tag, out var result))
            {
                return result;
            }
            if (TryReadDouble(tag, out var doubleVal))
            {
                return doubleVal.ToString();
            }
            if (TryReadInt(tag, out var intResult))
            {
                return intResult.ToString();
            }

            if (TryReadBool(tag, out var boolResult))
            {
                return boolResult ? "1" : "0";
            }

            if (TryReadLong(tag, out var longResult))
            {
                return longResult.ToString();
            }

            if (TryReadDate(tag, out var dateResult))
            {
                return dateResult.ToString();
            }

            throw RaiseError("string", tag);
        }

        public string ReadType()
        {
            var tag = _reader.ReadByte();
            if (TryReadString(tag, out var type))
            {
                types.Add(type);
                return type;
            }

            if (!TryReadInt(tag, out var index))
                throw RaiseError("type", tag);
            if (index < types.Count)
            {
                return types[index];
            }
            throw new IndexOutOfRangeException($"type ref #{index} is greater than the number of valid types ({types.Count})");
        }

        private bool TryReadList(int tag, out IList result)
        {
            switch (tag)
            {
                case Constants.VariableList:
                    var type = ReadType();
                    break;
                case Constants.VariableUnTypeList:
                    break;
            }
            //    case BC_LIST_VARIABLE:
            //        {
            //            // variable length list
            //            String type = readType();

            //            return findSerializerFactory().readList(this, -1, type);
            //        }

            //    case BC_LIST_VARIABLE_UNTYPED:
            //        {
            //            return findSerializerFactory().readList(this, -1, null);
            //        }

            //    case BC_LIST_FIXED:
            //        {
            //            // fixed length lists
            //            String type = readType();
            //            int length = readInt();

            //            Deserializer reader;
            //            reader = findSerializerFactory().getListDeserializer(type, null);

            //            return reader.readLengthList(this, length);
            //        }

            //    case BC_LIST_FIXED_UNTYPED:
            //        {
            //            // fixed length lists
            //            int length = readInt();

            //            Deserializer reader;
            //            reader = findSerializerFactory().getListDeserializer(null, null);

            //            return reader.readLengthList(this, length);
            //        }

            //    // compact fixed list
            //    case 0x70:
            //    case 0x71:
            //    case 0x72:
            //    case 0x73:
            //    case 0x74:
            //    case 0x75:
            //    case 0x76:
            //    case 0x77:
            //        {
            //            // fixed length lists
            //            String type = readType();
            //            int length = tag - 0x70;

            //            Deserializer reader;
            //            reader = findSerializerFactory().getListDeserializer(type, null);

            //            return reader.readLengthList(this, length);
            //        }

            //    // compact fixed untyped list
            //    case 0x78:
            //    case 0x79:
            //    case 0x7a:
            //    case 0x7b:
            //    case 0x7c:
            //    case 0x7d:
            //    case 0x7e:
            //    case 0x7f:
            //        {
            //            // fixed length lists
            //            int length = tag - 0x78;

            //            Deserializer reader;
            //            reader = findSerializerFactory().getListDeserializer(null, null);

            //            return reader.readLengthList(this, length);
            //        }
            result = null;
            return false;
        }

        public int ReadListStart()
        {
            return _reader.ReadByte();
        }

        public bool HasEnd()
        {
            int code = _reader.ReadByte();
            if (code < 0)
                return true;
            _reader.Seek(-1, SeekOrigin.Current);
            return code == Constants.End;
        }

        public void ReadToEnd()
        {
            var code = _reader.ReadByte();
            if (code == Constants.End)
            {
                return;
            }

            throw RaiseError("list/map end", code);
        }

        public int AddRef(object obj)
        {
            refs.Add(obj);
            return refs.Count - 1;
        }

        public object ReadObject()
        {
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return null;
            }

            if (TryReadBool(tag, out var boolVal))
            {
                return boolVal;
            }

            if (TryReadInt(tag, out var intVal))
            {
                return intVal;
            }

            if (TryReadLong(tag, out var longVal))
            {
                return longVal;
            }

            if (TryReadDouble(tag, out var doubleVal))
            {
                return doubleVal;
            }

            if (TryReadDate(tag, out var dateVal))
            {
                return dateVal;
            }

            if (TryReadString(tag, out var strVal))
            {
                return strVal;
            }

            if (TryReadBytes(tag, out var bytesVal))
            {
                return bytesVal;
            }



            //    case 'H':
            //        {
            //            return findSerializerFactory().readMap(this, null);
            //        }

            //    case 'M':
            //        {
            //            String type = readType();

            //            return findSerializerFactory().readMap(this, type);
            //        }

            //    case 'C':
            //        {
            //            readObjectDefinition(null);

            //            return readObject();
            //        }

            //    case 0x60:
            //    case 0x61:
            //    case 0x62:
            //    case 0x63:
            //    case 0x64:
            //    case 0x65:
            //    case 0x66:
            //    case 0x67:
            //    case 0x68:
            //    case 0x69:
            //    case 0x6a:
            //    case 0x6b:
            //    case 0x6c:
            //    case 0x6d:
            //    case 0x6e:
            //    case 0x6f:
            //        {
            //            int ref = tag - 0x60;

            //            if (_classDefs == null)
            //                throw error("No classes defined at reference '{0}'" + tag);

            //            ObjectDefinition def = (ObjectDefinition)_classDefs.get(ref);

            //            return readObjectInstance(null, def);
            //        }

            //    case 'O':
            //        {
            //            int ref = readInt();

            //            ObjectDefinition def = (ObjectDefinition)_classDefs.get(ref);

            //            return readObjectInstance(null, def);
            //        }

            //    case BC_REF:
            //        {
            //            int ref = readInt();

            //            return _refs.get(ref);
            //        }

            //    default:
            //        if (tag < 0)
            //            throw new EOFException("readObject: unexpected end of file");
            //        else
            throw RaiseError("object", tag);
        }

        public T ReadObject<T>()
        {
            throw new NotImplementedException();
        }
    }
}
