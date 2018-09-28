using System;

namespace Hessian.Lite.Serialize
{
    public class BasicSerializer : IHessianSerializer
    {
        private readonly BasicType _type;

        private static bool IsRefrenceType(BasicType type)
        {
            switch (type)
            {
                case BasicType.String:
                case BasicType.Object:
                case BasicType.BoolArray:
                case BasicType.ByteArray:
                case BasicType.SByteArray:
                case BasicType.ShortArray:
                case BasicType.UShortArray:
                case BasicType.IntArray:
                case BasicType.UIntArray:
                case BasicType.LongArray:
                case BasicType.ULongArray:
                case BasicType.FloatArray:
                case BasicType.DoubleArray:
                case BasicType.StringArray:
                case BasicType.ObjectArray:
                    return true;
                default:
                    return false;
            }
        }
        public BasicSerializer(BasicType type)
        {
            _type = type;
        }
        public void WriteObject(object obj, HessianWriter writer)
        {
            if (IsRefrenceType(_type) && writer.WriteRef(obj))
            {
                return;
            }
            switch (_type)
            {
                case BasicType.Null:
                    writer.WriteNull();
                    break;
                case BasicType.Bool:
                    writer.WriteBool((bool)obj);
                    break;
                case BasicType.Byte:
                case BasicType.SByte:
                case BasicType.Short:
                case BasicType.UShort:
                case BasicType.Int:
                    writer.WriteInt(Convert.ToInt32(obj));
                    break;
                case BasicType.UInt:
                case BasicType.Long:
                    writer.WriteLong(Convert.ToInt64(obj));
                    break;
                case BasicType.ULong:
                    break;
                case BasicType.Float:
                case BasicType.Double:
                    writer.WriteDouble(Convert.ToDouble(obj));
                    break;
                case BasicType.Char:
                    writer.WriteChars(new[] { (char)obj });
                    break;
                case BasicType.CharArray:
                    writer.WriteChars((char[])obj);
                    break;
                case BasicType.String:
                    writer.WriteString(Convert.ToString(obj));
                    break;
                case BasicType.Date:
                    writer.WriteDateTime(Convert.ToDateTime(obj));
                    break;
                case BasicType.Object:
                    writer.WriteObject(obj);
                    break;
                case BasicType.BoolArray:
                    var boolArray = (bool[])obj;
                    writer.WriteListStart(boolArray.Length, "[boolean");
                    foreach (var s in boolArray)
                    {
                        writer.WriteBool(s);
                    }
                    break;
                case BasicType.ShortArray:
                    var shortArray = (short[])obj;
                    writer.WriteListStart(shortArray.Length, "[short");
                    foreach (var s in shortArray)
                    {
                        writer.WriteInt(s);
                    }
                    break;
                case BasicType.IntArray:
                    var intArray = (int[])obj;
                    writer.WriteListStart(intArray.Length, "[int");
                    foreach (var s in intArray)
                    {
                        writer.WriteInt(s);
                    }
                    break;
                case BasicType.LongArray:
                    var longArray = (long[])obj;
                    writer.WriteListStart(longArray.Length, "[long");
                    foreach (var s in longArray)
                    {
                        writer.WriteLong(s);
                    }
                    break;
                case BasicType.FloatArray:
                    var floatArray = (float[])obj;
                    writer.WriteListStart(floatArray.Length, "[float");
                    foreach (var s in floatArray)
                    {
                        writer.WriteDouble(s);
                    }
                    break;
                case BasicType.DoubleArray:
                    var doubleArray = (double[])obj;
                    writer.WriteListStart(doubleArray.Length, "[double");
                    foreach (var s in doubleArray)
                    {
                        writer.WriteDouble(s);
                    }
                    break;
                case BasicType.StringArray:
                    var stringArray = (string[])obj;
                    writer.WriteListStart(stringArray.Length, "[string");
                    foreach (var s in stringArray)
                    {
                        writer.WriteString(s);
                    }
                    break;
                case BasicType.ByteArray:
                    var byteArray = (byte[])obj;
                    writer.WriteBytes(byteArray);
                    break;
                case BasicType.SByteArray:
                    var sbyteArray = (int[])obj;
                    writer.WriteListStart(sbyteArray.Length, "[int");
                    foreach (var s in sbyteArray)
                    {
                        writer.WriteInt(s);
                    }
                    break;
                case BasicType.UShortArray:
                    var ushortArray = (int[])obj;
                    writer.WriteListStart(ushortArray.Length, "[int");
                    foreach (var s in ushortArray)
                    {
                        writer.WriteInt(s);
                    }
                    break;
                case BasicType.UIntArray:
                    var uintArray = (uint[])obj;
                    writer.WriteListStart(uintArray.Length, "[long");
                    foreach (var s in uintArray)
                    {
                        writer.WriteLong(s);
                    }
                    break;
                case BasicType.ObjectArray:
                    var objArray = (object[])obj;
                    writer.WriteListStart(objArray.Length, "[object");
                    foreach (var item in objArray)
                    {
                        writer.WriteObject(item);
                    }
                    break;
                case BasicType.ULongArray:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}