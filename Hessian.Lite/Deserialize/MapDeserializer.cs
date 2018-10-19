using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Hessian.Lite.Deserialize
{
    public class MapDeserializer : AbstractDeserializer
    {
        private readonly Func<object> _creator;
        private readonly Func<MapDeserializer, Hessian2Reader, object> _genericConverter;
        private readonly Func<Hessian2Reader, IDictionary> _converter;
        private static readonly Type SelfType = typeof(MapDeserializer);
        private static readonly MethodInfo BaseMethodInfo = SelfType.GetMethod("ReadGenericDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

        public MapDeserializer(Type type)
        {
            Type = type;
            if (type.IsInterface)
            {
                type = type.IsGenericType
                    ? typeof(Dictionary<,>).MakeGenericType(type.GenericTypeArguments)
                    : typeof(Hashtable);
            }

            _creator = type.GetCreator();
            if (type.IsGenericType)
            {
                var method = BaseMethodInfo.MakeGenericMethod(type.GenericTypeArguments);
                var paramters = new List<ParameterExpression> { Expression.Parameter(typeof(Hessian2Reader), "reader") };
                var instanceParamter = Expression.Parameter(SelfType, "instance");
                var call = Expression.Call(instanceParamter, method, paramters);
                paramters.Insert(0, instanceParamter);
                _genericConverter = Expression.Lambda<Func<MapDeserializer, Hessian2Reader, object>>(call, paramters).Compile();
            }
            else
            {
                _converter = ReadDictionary;
            }
        }

        private IDictionary<TKey, TValue> ReadGenericDictionary<TKey, TValue>(Hessian2Reader reader)
        {
            var result = (IDictionary<TKey, TValue>)_creator();
            reader.AddRef(result);
            while (!reader.HasEnd())
            {
                result.Add(reader.ReadObject<TKey>(), reader.ReadObject<TValue>());
            }
            reader.ReadToEnd();
            return result;
        }

        private IDictionary ReadDictionary(Hessian2Reader reader)
        {
            var result = (IDictionary)_creator();
            reader.AddRef(result);
            while (!reader.HasEnd())
            {
                result.Add(reader.ReadObject(), reader.ReadObject());
            }
            reader.ReadToEnd();
            return result;
        }

        public override object ReadMap(Hessian2Reader reader)
        {
            return _genericConverter == null ? _converter(reader) : _genericConverter(this, reader);
        }

        public override object ReadObject(Hessian2Reader reader, ObjectDefinition definition)
        {
            if (Type.IsGenericType)
            {
                var dic = (IDictionary<string, object>)_creator();
                reader.AddRef(dic);
                foreach (var field in definition.Fields)
                {
                    dic.Add(field, reader.ReadObject());
                }
                dic.Add("$Type", definition.Type);
                return dic;
            }
            var hashTable = (IDictionary)_creator();
            reader.AddRef(hashTable);
            foreach (var field in definition.Fields)
            {
                hashTable.Add(field, reader.ReadObject());
            }
            hashTable.Add("$Type", definition.Type);
            return hashTable;
        }
    }
}