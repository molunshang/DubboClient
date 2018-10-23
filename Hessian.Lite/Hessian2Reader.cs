using Hessian.Lite.Exception;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hessian.Lite.Serialize;
using Hessian.Lite.Util;

namespace Hessian.Lite
{
    public class Hessian2Reader
    {
        private class BufferReadStream : Stream
        {
            private readonly Stream _innerStream;

            private readonly int _bufferLength;
            private byte[] _innerBuffer;
            private int _readLength;
            private int _offset;

            public override bool CanRead => _innerStream.CanRead;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => _innerStream.Length;

            public override long Position
            {
                get => _innerStream.Position - _readLength + _offset;
                set
                {
                    var offset = (int)(value - _innerStream.Position);
                    if (offset > 0 || offset < -_readLength)
                    {
                        throw new InvalidOperationException($"this stream position must between {(_innerStream.Position - _readLength).ToString()} and {_innerStream.Position.ToString()}");
                    }

                    _offset += offset;
                }
            }


            public BufferReadStream(Stream stream, int size = 4096)
            {
                _innerStream = stream;
                _bufferLength = size;
                _innerBuffer = ArrayPool<byte>.Shared.Rent(size);
            }


            private bool disposed = false;
            protected override void Dispose(bool disposing)
            {
                if (disposed)
                {
                    return;
                }
                ArrayPool<byte>.Shared.Return(_innerBuffer);
                _innerBuffer = null;
                disposed = true;
            }

            public override int ReadByte()
            {
                if (_offset >= _readLength)
                {
                    _readLength = _innerStream.Read(_innerBuffer, 0, _bufferLength);
                    _offset = 0;
                }

                if (_readLength == _offset)
                {
                    return -1;
                }
                return _innerBuffer[_offset++];

            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var bufferLength = _readLength - _offset;
                if (bufferLength > 0)
                {
                    if (bufferLength < count)
                    {
                        Array.Copy(_innerBuffer, _offset, buffer, offset, bufferLength);
                        offset += bufferLength;
                        count -= bufferLength;
                    }
                    else
                    {
                        Array.Copy(_innerBuffer, _offset, buffer, offset, count);
                        _offset += count;
                        return count;
                    }
                }

                _offset = _readLength = 0;
                return _innerStream.Read(buffer, offset, count) + bufferLength;
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }

        private readonly Stream _reader;
        private readonly List<string> types;
        private readonly List<object> refs;
        private readonly List<ObjectDefinition> defs;
        public Hessian2Reader(Stream reader)
        {
            _reader = reader.CanSeek ? reader : new BufferReadStream(reader);
            types = new List<string>();
            refs = new List<object>();
            defs = new List<ObjectDefinition>();
        }

        private HessianException RaiseError(string msg, int tag)
        {
            throw new HessianException($"unknown code {tag} where read {msg}");
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
                    result = DateTimeUtils.UtcStartTime.AddMilliseconds(_reader.ReadLong()).ToLocalTime();
                    return true;
                case Constants.DateTimeMinute:
                    var seconds = _reader.ReadInt();
                    result = DateTimeUtils.UtcStartTime.AddMinutes(seconds).ToLocalTime();
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
                    length = ((tag - Constants.StringMediumStart) << 8) + _reader.ReadByte();
                    break;
                default:
                    result = null;
                    return false;
            }

            result = _reader.ReadUtf8String(length);
            return true;
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
                            _reader.ReadBytes(buffer, length);
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
            _reader.ReadBytes(result);
            return true;
        }

        private bool TryReadList(int tag, out object list)
        {
            IHessianDeserializer deserializer;
            switch (tag)
            {
                case Constants.VariableList:
                    deserializer = SerializeFactory.GetListDeserializer(ReadType());
                    list = deserializer.ReadList(this, -1);
                    break;
                case Constants.VariableUnTypeList:
                    deserializer = SerializeFactory.GetListDeserializer(null);
                    list = deserializer.ReadList(this, -1);
                    break;
                case Constants.FixedList:
                    deserializer = SerializeFactory.GetListDeserializer(ReadType());
                    list = deserializer.ReadList(this, ReadInt());
                    break;
                case Constants.FixedUnTypeList:
                    deserializer = SerializeFactory.GetListDeserializer(null);
                    list = deserializer.ReadList(this, ReadInt());
                    break;
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                    deserializer = SerializeFactory.GetListDeserializer(ReadType());
                    list = deserializer.ReadList(this, tag - 0x70);
                    break;
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f:
                    deserializer = SerializeFactory.GetListDeserializer(null);
                    list = deserializer.ReadList(this, tag - 0x78);
                    break;
                default:
                    list = null;
                    return false;
            }

            return true;
        }

        private bool TryReadMap(int tag, out object map)
        {
            IHessianDeserializer deserializer;
            switch (tag)
            {
                case Constants.UnTypeMap:
                    deserializer = SerializeFactory.GetDeserializer(SerializeFactory.DefaultDictionaryType);
                    map = deserializer.ReadMap(this);
                    return true;
                case Constants.Map:
                    deserializer = SerializeFactory.GetDeserializer(ReadType(), SerializeFactory.DefaultDictionaryType);
                    map = deserializer.ReadMap(this);
                    return true;
                default:
                    map = null;
                    return false;
            }
        }

        private bool TryReadObject(int tag, out object obj)
        {
            switch (tag)
            {
                case Constants.ClassDef:
                    ReadObjectDefinition();
                    obj = ReadObject();
                    return true;
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6e:
                case 0x6f:
                    var refIndex = tag - 0x60;
                    if (defs.Count <= refIndex)
                        throw new HessianException($"class ref #{refIndex} is greater than the number of valid class defs ({defs.Count})");
                    obj = ReadObjectInstance(null, defs[refIndex]);
                    return true;
                case Constants.Object:
                    refIndex = ReadInt();
                    if (defs.Count <= refIndex)
                        throw new HessianException($"class ref #{refIndex} is greater than the number of valid class defs ({defs.Count})");
                    obj = ReadObjectInstance(null, defs[refIndex]);
                    return true;
                case Constants.Ref:
                    refIndex = ReadInt();
                    if (refs.Count <= refIndex)
                        throw new HessianException($"object ref #{refIndex} is greater than the number of valid object refs ({refs.Count})");
                    obj = refs[refIndex];
                    return true;
                default:
                    obj = null;
                    return false;
            }
        }

        private object ReadObjectInstance(Type type, ObjectDefinition def)
        {
            var deserializer = SerializeFactory.GetDeserializer(def.Type, type);
            return deserializer.ReadObject(this, def);
        }

        public void ReadObjectDefinition()
        {
            var type = ReadString();
            var len = ReadInt();

            var fieldNames = new string[len];
            for (var i = 0; i < len; i++)
                fieldNames[i] = ReadString();

            var def = new ObjectDefinition(type, fieldNames);
            defs.Add(def);
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
            throw new HessianException($"type ref #{index} is greater than the number of valid types ({types.Count})");
        }

        public int ReadListStart()
        {
            return _reader.ReadByte();
        }

        public bool HasEnd()
        {
            var code = _reader.ReadByte();
            if (code < 0)
                return true;
            _reader.Position--;
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

        public void ReplaceRef(int oldIndex, object newObj)
        {
            refs[oldIndex] = newObj;
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

            if (TryReadList(tag, out var result))
            {
                return result;
            }

            if (TryReadMap(tag, out result))
            {
                return result;
            }

            if (TryReadObject(tag, out result))
            {
                return result;
            }

            throw RaiseError("object", tag);
        }

        public object ReadObject(Type type)
        {
            if (type == typeof(object))
            {
                return ReadObject();
            }
            var tag = _reader.ReadByte();
            IHessianDeserializer deserializer;
            switch (tag)
            {
                case Constants.Null:
                    return type.IsPrimitive ? type.Default() : null;
                case Constants.UnTypeMap:
                    deserializer = SerializeFactory.GetDeserializer(type);
                    return deserializer.ReadMap(this);
                case Constants.Map:
                    var typeName = ReadType();
                    deserializer = string.IsNullOrEmpty(typeName) ? SerializeFactory.GetDeserializer(type) : SerializeFactory.GetDeserializer(typeName, type);
                    return deserializer.ReadMap(this);
                case Constants.ClassDef:
                    ReadObjectDefinition();
                    return ReadObject(type);
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6e:
                case 0x6f:
                    var refIndex = tag - 0x60;
                    return ReadObjectInstance(type, defs[refIndex]);
                case Constants.Object:
                    refIndex = ReadInt();
                    return ReadObjectInstance(type, defs[refIndex]);
                case Constants.VariableList:
                    typeName = ReadType();
                    deserializer = SerializeFactory.GetListDeserializer(typeName, type);
                    return deserializer.ReadList(this, -1);
                case Constants.FixedList:
                    typeName = ReadType();
                    deserializer = SerializeFactory.GetListDeserializer(typeName, type);
                    return deserializer.ReadList(this, ReadInt());
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                    typeName = ReadType();
                    deserializer = SerializeFactory.GetListDeserializer(typeName, type);
                    return deserializer.ReadList(this, tag - 0x70);
                case Constants.VariableUnTypeList:
                    deserializer = SerializeFactory.GetListDeserializer(null, type);
                    return deserializer.ReadList(this, -1);
                case Constants.FixedUnTypeList:
                    deserializer = SerializeFactory.GetListDeserializer(null, type);
                    return deserializer.ReadList(this, ReadInt());
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f:
                    deserializer = SerializeFactory.GetListDeserializer(null, type);
                    return deserializer.ReadList(this, tag - 0x78);
                case Constants.Ref:
                    refIndex = ReadInt();
                    return refs[refIndex];
                default:
                    _reader.Position--;
                    deserializer = SerializeFactory.GetDeserializer(type);
                    return deserializer.ReadObject(this);
            }
        }

        public T ReadObject<T>()
        {
            var type = typeof(T);
            if (type == typeof(object))
            {
                return (T)ReadObject();
            }
            var tag = _reader.ReadByte();
            if (tag == Constants.Null)
            {
                return default(T);
            }

            _reader.Position--;
            return (T)ReadObject(type);
        }

    }
}
