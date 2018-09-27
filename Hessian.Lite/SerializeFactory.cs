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
            else
            {

            }

            RegisterSerializer(type, serializer);
            return serializer;
        }
    }
}
