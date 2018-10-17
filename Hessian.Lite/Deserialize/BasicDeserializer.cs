using Hessian.Lite.Serialize;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Hessian.Lite.Deserialize
{
    public class BasicDeserializer : AbstractDeserializer
    {
        private readonly BasicType _type;

        public BasicDeserializer(BasicType type)
        {
            _type = type;
            Type = GetBasicType();
        }

        private Type GetBasicType()
        {
            switch (_type)
            {
                case BasicType.Null:
                    return typeof(void);
                case BasicType.Bool:
                    return typeof(bool);
                case BasicType.Byte:
                    return typeof(byte);
                case BasicType.SByte:
                    return typeof(sbyte);
                case BasicType.Short:
                    return typeof(short);
                case BasicType.UShort:
                    return typeof(ushort);
                case BasicType.Int:
                    return typeof(int);
                case BasicType.UInt:
                    return typeof(uint);
                case BasicType.Long:
                    return typeof(long);
                case BasicType.Float:
                    return typeof(float);
                case BasicType.Double:
                    return typeof(double);
                case BasicType.Char:
                    return typeof(char);
                case BasicType.String:
                    return typeof(string);
                case BasicType.Date:
                    return typeof(DateTime);
                case BasicType.Object:
                    return typeof(object);
                case BasicType.CharArray:
                    return typeof(char[]);
                case BasicType.BoolArray:
                    return typeof(bool[]);
                case BasicType.ByteArray:
                    return typeof(byte[]);
                case BasicType.SByteArray:
                    return typeof(sbyte[]);
                case BasicType.ShortArray:
                    return typeof(short[]);
                case BasicType.UShortArray:
                    return typeof(ushort[]);
                case BasicType.IntArray:
                    return typeof(int[]);
                case BasicType.UIntArray:
                    return typeof(uint[]);
                case BasicType.LongArray:
                    return typeof(long[]);
                case BasicType.FloatArray:
                    return typeof(float[]);
                case BasicType.DoubleArray:
                    return typeof(double[]);
                case BasicType.StringArray:
                    return typeof(string[]);
                case BasicType.DateArray:
                    return typeof(DateTime[]);
                case BasicType.ObjectArray:
                    return typeof(object[]);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public override object ReadObject(Hessian2Reader reader)
        {
            switch (_type)
            {
                case BasicType.Null:
                    return reader.ReadObject();
                case BasicType.Bool:
                    return reader.ReadBool();
                case BasicType.Byte:
                    return (byte)reader.ReadInt();
                case BasicType.SByte:
                    return (sbyte)reader.ReadInt();
                case BasicType.Short:
                    return reader.ReadShort();
                case BasicType.UShort:
                    return (ushort)reader.ReadInt();
                case BasicType.Int:
                    return reader.ReadInt();
                case BasicType.UInt:
                    return (uint)reader.ReadLong();
                case BasicType.Long:
                    return reader.ReadLong();
                case BasicType.Float:
                    return reader.ReadFloat();
                case BasicType.Double:
                    return reader.ReadDouble();
                case BasicType.Char:
                    var str = reader.ReadString();
                    return string.IsNullOrEmpty(str) ? (char)0 : str[0];
                case BasicType.CharArray:
                    var charsString = reader.ReadString();
                    return string.IsNullOrEmpty(charsString) ? new char[0] : charsString.ToCharArray();
                case BasicType.String:
                    return reader.ReadString();
                case BasicType.Date:
                    return reader.ReadDate();
                case BasicType.ByteArray:
                    return reader.ReadBytes();
                case BasicType.Object:
                    return reader.ReadObject();
                case BasicType.BoolArray:
                case BasicType.ShortArray:
                case BasicType.IntArray:
                case BasicType.LongArray:
                case BasicType.FloatArray:
                case BasicType.StringArray:
                case BasicType.DoubleArray:
                case BasicType.SByteArray:
                case BasicType.UShortArray:
                case BasicType.UIntArray:
                case BasicType.DateArray:
                case BasicType.ObjectArray:
                    return reader.ReadObject();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private T[] ReadArray<T>(Hessian2Reader reader, int length, Func<T> func)
        {
            var result = new T[length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = func();
            }

            reader.AddRef(result);
            return result;
        }

        private T[] ReadList<T>(Hessian2Reader reader, Func<T> func)
        {
            var list = new List<T>();
            while (!reader.HasEnd())
            {
                list.Add(func());
            }
            reader.ReadToEnd();
            var result = list.ToArray();
            reader.AddRef(result);
            return result;
        }
        public override object ReadList(Hessian2Reader reader, int length)
        {
            if (length >= 0)
            {
                switch (_type)
                {
                    case BasicType.BoolArray:
                        return ReadArray(reader, length, reader.ReadBool);
                    case BasicType.SByteArray:
                        return ReadArray(reader, length, () => (sbyte)reader.ReadInt());
                    case BasicType.ShortArray:
                        return ReadArray(reader, length, reader.ReadShort);
                    case BasicType.UShortArray:
                        return ReadArray(reader, length, () => (ushort)reader.ReadInt());
                    case BasicType.IntArray:
                        return ReadArray(reader, length, reader.ReadInt);
                    case BasicType.UIntArray:
                        return ReadArray(reader, length, () => (uint)reader.ReadLong());
                    case BasicType.LongArray:
                        return ReadArray(reader, length, reader.ReadLong);
                    case BasicType.FloatArray:
                        return ReadArray(reader, length, reader.ReadFloat);
                    case BasicType.DoubleArray:
                        return ReadArray(reader, length, reader.ReadDouble);
                    case BasicType.StringArray:
                        return ReadArray(reader, length, reader.ReadString);
                    case BasicType.DateArray:
                        return ReadArray(reader, length, reader.ReadDate);
                    case BasicType.ObjectArray:
                        return ReadArray(reader, length, reader.ReadObject);
                }
            }
            else
            {
                switch (_type)
                {
                    case BasicType.BoolArray:
                        return ReadList(reader, reader.ReadBool);
                    case BasicType.SByteArray:
                        return ReadList(reader, () => (sbyte)reader.ReadInt());
                    case BasicType.ShortArray:
                        return ReadList(reader, reader.ReadShort);
                    case BasicType.UShortArray:
                        return ReadList(reader, () => (ushort)reader.ReadInt());
                    case BasicType.IntArray:
                        return ReadList(reader, reader.ReadInt);
                    case BasicType.UIntArray:
                        return ReadList(reader, () => (uint)reader.ReadLong());
                    case BasicType.LongArray:
                        return ReadList(reader, reader.ReadLong);
                    case BasicType.FloatArray:
                        return ReadList(reader, reader.ReadFloat);
                    case BasicType.DoubleArray:
                        return ReadList(reader, reader.ReadDouble);
                    case BasicType.StringArray:
                        return ReadList(reader, reader.ReadString);
                    case BasicType.DateArray:
                        return ReadList(reader, reader.ReadDate);
                    case BasicType.ObjectArray:
                        return ReadList(reader, reader.ReadObject);
                }
            }
            throw new NotSupportedException();
        }
    }
}