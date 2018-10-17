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
            var dic = (IDictionary<TKey, TValue>)_creator();
            while (!reader.HasEnd())
            {
                dic.Add(reader.ReadObject<TKey>(), reader.ReadObject<TValue>());
            }
            reader.ReadToEnd();
            return dic;
        }

        private IDictionary ReadDictionary(Hessian2Reader reader)
        {
            var dic = (IDictionary)_creator();
            while (!reader.HasEnd())
            {
                dic.Add(reader.ReadObject(), reader.ReadObject());
            }
            reader.ReadToEnd();
            return dic;
        }


        public override object ReadMap(Hessian2Reader reader)
        {
            var result = _genericConverter == null ? _converter(reader) : _genericConverter(this, reader);
            reader.AddRef(result);
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            object result;
            if (Type.IsGenericType)
            {
                var dic = (IDictionary<string, object>)_creator();
                foreach (var t in fieldNames)
                {
                    dic.Add(t, reader.ReadObject());
                }

                result = dic;
            }
            else
            {
                var hashTable = (IDictionary)_creator();
                foreach (var t in fieldNames)
                {
                    hashTable.Add(t, reader.ReadObject());
                }

                result = hashTable;
            }
            reader.AddRef(result);
            return result;
        }
    }
}