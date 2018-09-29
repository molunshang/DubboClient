using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace Hessian.Lite
{
    public class Hessian2Writer
    {
        private readonly Stream _stream;
        private readonly Dictionary<string, int> _typeRefs = new Dictionary<string, int>();
        private readonly Dictionary<object, int> _objRefs = new Dictionary<object, int>();

        public Hessian2Writer(Stream output)
        {
            _stream = output;
        }

        protected void WriteType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("the type is not allow null or empty string");
            }

            type = SerializeFactory.GetMapType(type);
            if (_typeRefs.TryGetValue(type, out var index))
            {
                WriteInt(index);
            }
            else
            {
                _typeRefs[type] = _typeRefs.Count;
                WriteString(type);
            }
        }

        public void WriteBytes(byte[] buffer)
        {
            WriteBytes(buffer, 0, buffer.Length);
        }

        public void WriteBytes(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                WriteNull();
                return;
            }

            while (count > Constants.BinaryChunkLength)
            {
                _stream.WriteByte(Constants.BinaryChunk);
                _stream.WriteByte(Constants.BinaryChunkLength >> 8);
                _stream.WriteByte(Constants.BinaryChunkLength & byte.MaxValue);
                _stream.Write(buffer, offset, Constants.BinaryChunkLength);
                count -= Constants.BinaryChunkLength;
                offset += Constants.BinaryChunkLength;
            }

            if (count <= Constants.BinaryChunkMinLength)
            {
                _stream.WriteByte((byte)(count + Constants.BinaryChunkMinStart));
            }
            else
            {
                _stream.WriteByte(Constants.BinaryFinalChunk);
                _stream.WriteByte((byte)(count >> 8));
                _stream.WriteByte((byte)count);
            }
            _stream.Write(buffer, offset, count);
        }

        public void WriteBool(bool value)
        {
            _stream.WriteByte(value ? Constants.True : Constants.False);
        }

        public void WriteDateTime(DateTime value)
        {
            var timeStamp = value.TimeStamp();
            if (timeStamp % Constants.MinuteTotalMilliseconds == 0)
            {
                var minutes = timeStamp / Constants.MinuteTotalMilliseconds;
                var headBit = minutes >> 31;
                if (headBit == 0 || headBit == -1)
                {
                    _stream.WriteByte(Constants.DateTimeMinute);
                    _stream.WriteInt(minutes);
                    return;
                }
            }
            _stream.WriteByte(Constants.DateTimeMillisecond);
            _stream.WriteLong(timeStamp);
        }

        public void WriteDouble(double value)
        {
            var intVal = (int)value;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (intVal == value)
            {
                if (intVal == 0)
                {
                    _stream.WriteByte(Constants.DoubleZero);
                }
                else if (intVal == 1)
                {
                    _stream.WriteByte(Constants.DoubleOne);
                }
                else if (intVal >= sbyte.MinValue && intVal <= sbyte.MaxValue)
                {
                    _stream.WriteByte(Constants.DoubleByte);
                    _stream.WriteByte((byte)intVal);
                }
                else if (intVal >= ushort.MinValue && intVal <= ushort.MaxValue)
                {
                    _stream.WriteByte(Constants.DoubleShort);
                    _stream.WriteByte((byte)(intVal >> 8));
                    _stream.WriteByte((byte)intVal);
                }
            }
            var longVal = BitConverter.DoubleToInt64Bits(value);
            _stream.WriteByte(Constants.Double);
            _stream.WriteLong(longVal);
        }

        public void WriteInt(int value)
        {
            if (value >= Constants.IntOneByteMin && value <= Constants.IntOneByteMax)
            {
                _stream.WriteByte((byte)(Constants.IntOneByte + value));
            }
            else if (value >= Constants.IntTwoByteMin && value <= Constants.IntTwoByteMax)
            {
                _stream.WriteByte((byte)(Constants.IntTwoByte + (value >> 8)));
                _stream.WriteByte((byte)value);
            }
            else if (value >= Constants.IntThreeMin && value <= Constants.IntThreeMax)
            {
                _stream.WriteByte((byte)(Constants.IntThreeByte + (value >> 16)));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(Constants.Int);
                _stream.WriteInt(value);
            }
        }

        public bool WriteListStart(int length, string type)
        {
            if (length < 0)
            {
                if (type != null)
                {
                    _stream.WriteByte(Constants.VariableList);
                    WriteType(type);
                }
                else
                {
                    _stream.WriteByte(Constants.VariableUnTypeList);
                }
                return true;
            }

            if (length <= Constants.SmallListMaxLength)
            {
                if (type != null)
                {
                    _stream.WriteByte((byte)(Constants.SmallFixedList + length));
                    WriteType(type);
                }
                else
                {
                    _stream.WriteByte((byte)(Constants.SmallFixedUnTypeList + length));
                }
                return false;
            }

            if (type != null)
            {
                _stream.WriteByte(Constants.FixedList);
                WriteType(type);
            }
            else
            {
                _stream.WriteByte(Constants.FixedUnTypeList);
            }
            WriteInt(length);
            return false;
        }

        public void WriteListEnd()
        {
            _stream.WriteByte(Constants.End);
        }
        public void WriteLong(long value)
        {
            if (value >= Constants.LongOneByteMin && value <= Constants.LongOneByteMax)
            {
                _stream.WriteByte((byte)(Constants.LongOneByte + value));
            }
            else if (value >= Constants.LongTwoByteMin && value <= Constants.LongTwoByteMax)
            {
                _stream.WriteByte((byte)(Constants.LongTwoByte + (value >> 8)));
                _stream.WriteByte((byte)value);
            }
            else if (value >= Constants.LongThreeMin && value <= Constants.LongThreeMax)
            {
                _stream.WriteByte((byte)(Constants.LongThreeByte + (value >> 16)));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else if (value >= int.MinValue && value <= int.MaxValue)
            {
                _stream.WriteByte(Constants.LongFourByte);
                _stream.WriteInt(value);
            }
            else
            {
                _stream.WriteByte(Constants.Long);
                _stream.WriteLong(value);
            }
        }

        public void WriteMapBegin(string type)
        {
            if (type == null)
            {
                _stream.WriteByte(Constants.UnTypeMap);
            }
            else
            {
                _stream.WriteByte(Constants.Map);
                WriteType(type);
            }
        }

        public void WriteMapEnd()
        {
            _stream.WriteByte(Constants.End);
        }

        public void WriteNull()
        {
            _stream.WriteByte(Constants.Null);
        }

        public void WriteObject(object obj)
        {
            if (obj == null)
            {
                WriteNull();
                return;
            }
            var serializer = SerializeFactory.GetSerializer(obj.GetType());
            serializer.WriteObject(obj, this);
        }

        public bool WriteRef(object obj)
        {
            if (_objRefs.TryGetValue(obj, out var index))
            {
                _stream.WriteByte(Constants.Ref);
                WriteInt(index);
                return true;
            }
            _objRefs.Add(obj, _objRefs.Count);
            return false;
        }

        public void WriteString(string str)
        {
            if (str == null)
            {
                WriteNull();
                return;
            }

            int offset = 0, length = str.Length;
            while (length > Constants.StringChunkLength)
            {
                var subLength = Constants.StringChunkLength;
                var tail = str[offset + subLength - 1];
                if (tail >= Constants.SurrogateMin && tail <= Constants.SurrogateMax)
                {
                    subLength--;
                }

                _stream.WriteByte(Constants.String);
                _stream.WriteByte((byte)(subLength >> 8));
                _stream.WriteByte((byte)subLength);
                _stream.WriteUtf8String(str, offset, subLength);
                length -= subLength;
                offset += subLength;
            }
            if (length <= Constants.StringSmallLength)
            {
                _stream.WriteByte((byte)length);
            }
            else if (length <= Constants.StringMediumLength)
            {
                _stream.WriteByte((byte)(Constants.StringMediumStart + (length >> 8)));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(Constants.StringFinal);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            _stream.WriteUtf8String(str, offset, length);
        }

        public void WriteChars(char[] chars)
        {
            if (chars == null)
            {
                WriteNull();
                return;
            }
            WriteString(new string(chars));
        }

        public void WriteStream(Stream stream)
        {
            if (stream == null)
            {
                WriteNull();
                return;
            }

            byte[] buffer = null;
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(1024);
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _stream.WriteByte(Constants.BinaryChunk);
                    _stream.WriteByte((byte)(count >> 8));
                    _stream.WriteByte((byte)count);
                    _stream.Write(buffer, 0, Constants.BinaryChunkLength);
                }
                _stream.WriteByte(Constants.BinaryChunkMinStart);
            }
            finally
            {
                if (buffer != null)
                    ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
