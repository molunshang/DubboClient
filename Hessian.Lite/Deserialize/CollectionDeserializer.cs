using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Hessian.Lite.Deserialize
{
    public class CollectionDeserializer : AbstractDeserializer
    {
        private static readonly Type SelfType = typeof(CollectionDeserializer);

        private static readonly MethodInfo BaseMethod =
            SelfType.GetMethod("ReadGenericList", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly Func<Hessian2Reader, int, object> _listReader;

        public CollectionDeserializer(Type type)
        {
            Type = type;
            if (type.IsGenericType)
            {
                var argType = type.GenericTypeArguments[0];
                var method = BaseMethod.MakeGenericMethod(type.IsInterface ? typeof(List<>).MakeGenericType(argType) : type, type.GenericTypeArguments[0]);
                var paramExpression = new[]
                {
                    Expression.Parameter(typeof(Hessian2Reader)),
                    Expression.Parameter(typeof(int))
                };
                var call = Expression.Call(method, paramExpression);
                _listReader = Expression.Lambda<Func<Hessian2Reader, int, object>>(call, paramExpression).Compile();
            }
            else
            {
                _listReader = ReadObjectList;
            }
        }

        private static ICollection<TItem> ReadGenericList<T, TItem>(Hessian2Reader reader, int length) where T : ICollection<TItem>, new()
        {
            ICollection<TItem> collection = new T();
            if (length >= 0)
            {
                for (int i = 0; i < length; i++)
                {
                    collection.Add(reader.ReadObject<TItem>());
                }
            }
            else
            {
                while (!reader.HasEnd())
                {
                    collection.Add(reader.ReadObject<TItem>());
                }
                reader.ReadToEnd();
            }

            return collection;
        }

        private ICollection ReadObjectList(Hessian2Reader reader, int length)
        {
            var collection = new ArrayList();
            if (length >= 0)
            {
                for (int i = 0; i < length; i++)
                {
                    collection.Add(reader.ReadObject());
                }
            }
            else
            {
                while (!reader.HasEnd())
                {
                    collection.Add(reader.ReadObject());
                }
                reader.ReadToEnd();
            }

            return collection;
        }

        public override object ReadList(Hessian2Reader reader, int length)
        {
            var result = _listReader(reader, length);
            reader.AddRef(reader);
            return result;
        }
    }
}