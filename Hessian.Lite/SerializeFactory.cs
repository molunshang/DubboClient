﻿using Hessian.Lite.Deserialize;
using Hessian.Lite.Serialize;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Hessian.Lite
{
    public class SerializeFactory
    {
        private static readonly ConcurrentDictionary<string, string> TypeMap = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<Type, IHessianSerializer> SerializerMap = new ConcurrentDictionary<Type, IHessianSerializer>();
        private static readonly ConcurrentDictionary<Type, IHessianDeserializer> DeserializerMap = new ConcurrentDictionary<Type, IHessianDeserializer>();
        private static readonly ConcurrentDictionary<string, IHessianDeserializer> TypeNameDeserializers = new ConcurrentDictionary<string, IHessianDeserializer>();

        public static Type DefaultCollectionType = typeof(ArrayList);

        public static Type DefaultDictionaryType = typeof(Hashtable);

        private static IHessianSerializer SingleSerializer<T>() where T : IHessianSerializer, new()
        {
            return SerializerMap.GetOrAdd(typeof(T), new T());
        }

        static SerializeFactory()
        {
            RegisterBasic(typeof(bool), "boolean", BasicType.Bool);
            RegisterBasic(typeof(byte), "byte", BasicType.Byte);
            RegisterBasic(typeof(sbyte), "sbyte", BasicType.SByte);
            RegisterBasic(typeof(short), "short", BasicType.Short);
            RegisterBasic(typeof(ushort), "ushort", BasicType.UShort);
            RegisterBasic(typeof(int), "int", BasicType.Int);
            RegisterBasic(typeof(uint), "uint", BasicType.UInt);
            RegisterBasic(typeof(long), "long", BasicType.Long);
            RegisterBasic(typeof(float), "float", BasicType.Float);
            RegisterBasic(typeof(double), "double", BasicType.Double);
            RegisterBasic(typeof(char), "char", BasicType.Char);
            RegisterBasic(typeof(string), "string", BasicType.String);
            RegisterBasic(typeof(DateTime), "date", BasicType.Date);
            RegisterBasic(typeof(object), "object", BasicType.Object);
            RegisterBasic(typeof(char[]), "[char", BasicType.CharArray);
            RegisterBasic(typeof(bool[]), "[boolean", BasicType.BoolArray);
            RegisterBasic(typeof(byte[]), "[byte", BasicType.ByteArray);
            RegisterBasic(typeof(sbyte[]), "[sbyte", BasicType.ByteArray);
            RegisterBasic(typeof(short[]), "[short", BasicType.ShortArray);
            RegisterBasic(typeof(ushort[]), "[ushort", BasicType.UShortArray);
            RegisterBasic(typeof(int[]), "[int", BasicType.IntArray);
            RegisterBasic(typeof(uint[]), "[uint", BasicType.UIntArray);
            RegisterBasic(typeof(long[]), "[long", BasicType.LongArray);
            RegisterBasic(typeof(float[]), "[float", BasicType.FloatArray);
            RegisterBasic(typeof(double[]), "[double", BasicType.DoubleArray);
            RegisterBasic(typeof(string[]), "[string", BasicType.StringArray);
            RegisterBasic(typeof(DateTime[]), "[date", BasicType.DateArray);
            RegisterBasic(typeof(object[]), "[object", BasicType.ObjectArray);

            var type = typeof(ulong);
            SerializerMap.TryAdd(type, SingleSerializer<StringSerializer>());
            DeserializerMap.TryAdd(type, new StringDeserializer(type, str => ulong.Parse(str)));
            type = typeof(decimal);
            SerializerMap.TryAdd(type, SingleSerializer<StringSerializer>());
            DeserializerMap.TryAdd(type, new StringDeserializer(type, str => decimal.Parse(str)));
        }

        static void RegisterBasic(Type type, string typeName, BasicType basicType)
        {
            SerializerMap.TryAdd(type, new BasicSerializer(basicType));
            var deserializer = new BasicDeserializer(basicType);
            DeserializerMap.TryAdd(type, deserializer);
            TypeNameDeserializers.TryAdd(typeName, deserializer);
        }
        public static bool RegisterDefaultTypeMap(string type, string mapType)
        {
            if (!TypeMap.TryAdd(type, mapType))
            {
                return false;
            }

            TypeMap.AddOrUpdate(mapType, type, (key, old) => type);
            return true;
        }
        public static bool RegisterDefaultTypeMap(Type type, string mapType)
        {
            return RegisterDefaultTypeMap(type.AssemblyQualifiedName, mapType);
        }
        public static bool TryGetMapType(string type, out string mapType)
        {
            return TypeMap.TryGetValue(type, out mapType);
        }

        public static IHessianSerializer GetSerializer(Type type)
        {
            if (SerializerMap.TryGetValue(type, out var serializer))
            {
                return serializer;
            }
            if (type.IsArray)
            {
                serializer = SingleSerializer<ArraySerializer>();
            }
            else if (type.IsSubType(typeof(Stream)))
            {
                serializer = SingleSerializer<StreamSerializer>();
            }
            else if (type.IsSubType(typeof(IDictionary)) || type.IsGenericType && type.GetGenericTypeDefinition().IsSubType(typeof(IDictionary<,>)))
            {
                serializer = SingleSerializer<MapSerializer>();
            }
            else if (type.IsSubType(typeof(ICollection)) || type.IsGenericType && type.GetGenericTypeDefinition().IsSubType(typeof(ICollection<>)))
            {
                serializer = SingleSerializer<CollectionSerializer>();
            }
            else if (type.IsSubType(typeof(IEnumerable)))
            {
                serializer = SingleSerializer<EnumerableSerializer>();
            }
            else if (type.IsSubType(typeof(IEnumerator)))
            {
                serializer = SingleSerializer<EnumeratorSerializer>();
            }
            else
            {
                serializer = new ObjectSerializer(type);
            }
            SerializerMap.TryAdd(type, serializer);
            return serializer;
        }

        public static IHessianDeserializer GetDeserializer(Type type)
        {
            if (DeserializerMap.TryGetValue(type, out var deserializer))
            {
                return deserializer;
            }

            if (type.IsArray)
            {
                deserializer = CreateArrayDeserializer(type);
            }
            else if (type.IsEnum)
            {
                deserializer = new EnumDeserializer(type);
            }
            else if (type.IsSubType(typeof(IDictionary)) ||
                     type.IsGenericType && type.GetGenericTypeDefinition().IsSubType(typeof(IDictionary<,>)))
            {
                deserializer = new MapDeserializer(type);
            }
            else if (type.IsSubType(typeof(ICollection)) ||
                     type.IsGenericType && type.GetGenericTypeDefinition().IsSubType(typeof(ICollection<>)))
            {
                deserializer = new CollectionDeserializer(type);
            }
            else if (type.IsSubType(typeof(Stream)))
            {
                deserializer = DeserializerMap.GetOrAdd(type, tkey => new StreamDeserializer());
            }
            else if (type.IsSubType(typeof(IEnumerable)))
            {
                deserializer = DeserializerMap.GetOrAdd(type, tkey => new EnumerableDeserializer(type));
            }
            else if (type.IsSubType(typeof(IEnumerator)))
            {
                deserializer = DeserializerMap.GetOrAdd(type, tkey => new EnumeratorDeserializer(type));
            }
            else if (type.IsInterface || type.IsAbstract)
            {

            }
            else
            {
                deserializer = CreateObjectDeserializer(type);
            }
            //todo 接口/抽象类
            //todo 默认解析类
            DeserializerMap.TryAdd(type, deserializer);
            return deserializer;
        }

        private static AbstractDeserializer CreateArrayDeserializer(Type type)
        {
            var deserializeType = typeof(ArrayDeserializer<>).MakeGenericType(type);
            return (AbstractDeserializer)Activator.CreateInstance(deserializeType);
        }

        private static AbstractDeserializer CreateObjectDeserializer(Type type)
        {
            var deserializeType = typeof(ObjectDeserializer<>).MakeGenericType(type);
            return (AbstractDeserializer)Activator.CreateInstance(deserializeType);
        }

        public static IHessianDeserializer GetDeserializer(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return DeserializerMap.GetOrAdd(DefaultDictionaryType, type => new MapDeserializer(DefaultDictionaryType));
            }

            if (TypeNameDeserializers.TryGetValue(typeName, out var deserializer))
            {
                return deserializer;
            }

            if (typeName.StartsWith("["))
            {
                var subDeserializer = GetDeserializer(typeName.Substring(1));
                deserializer = CreateArrayDeserializer(subDeserializer.Type);
            }
            else
            {
                var targetType = Type.GetType(typeName);
                if (targetType != null)
                {
                    deserializer = GetDeserializer(targetType);
                }

                if (deserializer == null)
                {
                    deserializer = DeserializerMap.GetOrAdd(DefaultDictionaryType, type => new MapDeserializer(DefaultDictionaryType));
                }
            }

            TypeNameDeserializers.TryAdd(typeName, deserializer);
            return deserializer;
        }

        public static IHessianDeserializer GetDeserializer(string type, Type targetType)
        {
            var deserializer = GetDeserializer(type);
            if (targetType == null || targetType == deserializer.Type || deserializer.Type.IsSubType(targetType))
            {
                return deserializer;
            }

            return GetDeserializer(targetType);
        }
    }
}
