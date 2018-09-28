using Hessian.Lite.Serialize;
using System;
using System.Collections;
using System.Collections.Concurrent;

namespace Hessian.Lite
{
    public static class SerializeFactory
    {
        private static readonly ConcurrentDictionary<string, string> TypeMap = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<Type, IHessianSerializer> SerializerMap = new ConcurrentDictionary<Type, IHessianSerializer>();

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
                serializer = ArraySerializer.Instance;
            }
            else if (type.IsAssignableFrom(typeof(ICollection)))
            {

            }
            else if (type.IsAssignableFrom(typeof(IEnumerable)))
            {

            }
            else if (type.IsAssignableFrom(typeof(IEnumerator)))
            {

            }
            else if (type.IsAssignableFrom(typeof(IDictionary)))
            {

            }
            RegisterSerializer(type, serializer);
            return serializer;
        }
    }
}
