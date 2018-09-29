using Hessian.Lite.Serialize;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;

namespace Hessian.Lite
{
    public static class SerializeFactory
    {
        private static readonly ConcurrentDictionary<string, string> TypeMap = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<Type, IHessianSerializer> SerializerMap = new ConcurrentDictionary<Type, IHessianSerializer>();

        private static IHessianSerializer SingleObject<T>() where T : IHessianSerializer, new()
        {
            return SerializerMap.GetOrAdd(typeof(T), new T());
        }

        static SerializeFactory()
        {
            RegisterSerializer(typeof(bool), new BasicSerializer(BasicType.Bool));
            RegisterSerializer(typeof(byte), new BasicSerializer(BasicType.Byte));
            RegisterSerializer(typeof(sbyte), new BasicSerializer(BasicType.SByte));
            RegisterSerializer(typeof(short), new BasicSerializer(BasicType.Short));
            RegisterSerializer(typeof(ushort), new BasicSerializer(BasicType.UShort));
            RegisterSerializer(typeof(int), new BasicSerializer(BasicType.Int));
            RegisterSerializer(typeof(uint), new BasicSerializer(BasicType.UInt));
            RegisterSerializer(typeof(long), new BasicSerializer(BasicType.Long));
            RegisterSerializer(typeof(ulong), new BasicSerializer(BasicType.ULong));
            RegisterSerializer(typeof(float), new BasicSerializer(BasicType.Float));
            RegisterSerializer(typeof(double), new BasicSerializer(BasicType.Double));
            RegisterSerializer(typeof(char), new BasicSerializer(BasicType.Char));
            RegisterSerializer(typeof(string), new BasicSerializer(BasicType.String));
            RegisterSerializer(typeof(DateTime), new BasicSerializer(BasicType.Date));
            RegisterSerializer(typeof(object), new BasicSerializer(BasicType.Object));
            RegisterSerializer(typeof(char[]), new BasicSerializer(BasicType.CharArray));
            RegisterSerializer(typeof(bool[]), new BasicSerializer(BasicType.BoolArray));
            RegisterSerializer(typeof(byte[]), new BasicSerializer(BasicType.ByteArray));
            RegisterSerializer(typeof(sbyte[]), new BasicSerializer(BasicType.ByteArray));
            RegisterSerializer(typeof(short[]), new BasicSerializer(BasicType.ShortArray));
            RegisterSerializer(typeof(ushort[]), new BasicSerializer(BasicType.UShortArray));
            RegisterSerializer(typeof(int[]), new BasicSerializer(BasicType.IntArray));
            RegisterSerializer(typeof(uint[]), new BasicSerializer(BasicType.UIntArray));
            RegisterSerializer(typeof(long[]), new BasicSerializer(BasicType.LongArray));
            RegisterSerializer(typeof(float[]), new BasicSerializer(BasicType.FloatArray));
            RegisterSerializer(typeof(double[]), new BasicSerializer(BasicType.DoubleArray));
            RegisterSerializer(typeof(string[]), new BasicSerializer(BasicType.StringArray));
            RegisterSerializer(typeof(DateTime[]), new BasicSerializer(BasicType.DateArray));
            RegisterSerializer(typeof(object[]), new BasicSerializer(BasicType.ObjectArray));
        }

        public static bool RegisterTypeMap(string type, string mapType)
        {
            if (!TypeMap.TryAdd(type, mapType))
            {
                return false;
            }

            TypeMap.AddOrUpdate(mapType, type, (key, old) => type);
            return true;
        }
        public static string GetMapType(string type)
        {
            return TypeMap.TryGetValue(type, out var mapType) ? mapType : type;
        }

        public static bool RegisterSerializer(Type type, IHessianSerializer serializer)
        {
            return SerializerMap.TryAdd(type, serializer);
        }
        public static IHessianSerializer GetSerializer(Type type)
        {
            if (SerializerMap.TryGetValue(type, out var serializer))
            {
                return serializer;
            }
            if (type.IsArray)
            {
                serializer = SingleObject<ArraySerializer>();
            }
            else if (type.IsSubType(typeof(Stream)))
            {
                serializer = SingleObject<StreamSerializer>();
            }
            else if (type.IsSubType(typeof(IDictionary)))
            {
                serializer = SingleObject<MapSerializer>();
            }
            else if (type.IsSubType(typeof(ICollection)))
            {
                serializer = SingleObject<CollectionSerializer>();
            }
            else if (type.IsSubType(typeof(IEnumerable)))
            {
                serializer = SingleObject<EnumerableSerializer>();
            }
            else if (type.IsSubType(typeof(IEnumerator)))
            {
                serializer = SingleObject<EnumeratorSerializer>();
            }
            else
            {

            }
            RegisterSerializer(type, serializer);
            return serializer;
        }
    }
}
