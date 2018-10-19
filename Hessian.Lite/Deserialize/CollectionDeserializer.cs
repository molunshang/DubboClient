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
            ICollection<TItem> result = new T();
            reader.AddRef(result);
            if (length >= 0)
            {
                for (int i = 0; i < length; i++)
                {
                    result.Add(reader.ReadObject<TItem>());
                }
            }
            else
            {
                while (!reader.HasEnd())
                {
                    result.Add(reader.ReadObject<TItem>());
                }
                reader.ReadToEnd();
            }
            return result;
        }

        private ICollection ReadObjectList(Hessian2Reader reader, int length)
        {
            var result = new ArrayList();
            reader.AddRef(result);
            if (length >= 0)
            {
                for (int i = 0; i < length; i++)
                {
                    result.Add(reader.ReadObject());
                }
            }
            else
            {
                while (!reader.HasEnd())
                {
                    result.Add(reader.ReadObject());
                }
                reader.ReadToEnd();
            }

            return result;
        }

        public override object ReadList(Hessian2Reader reader, int length)
        {
            return _listReader(reader, length);
        }
    }
}