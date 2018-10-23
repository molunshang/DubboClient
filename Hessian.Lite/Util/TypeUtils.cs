using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Hessian.Lite.Util
{
    public static class TypeUtils
    {
        public static readonly Type[] EmptyArguments = new Type[0];
        private static readonly ConcurrentDictionary<Type, object> DefaultValue = new ConcurrentDictionary<Type, object>();
        public static bool IsSubType(this Type child, Type parent)
        {
            return parent.IsAssignableFrom(child) || child.GetInterfaces().Any(t => t == parent);
        }

        public static Func<T> GetCreator<T>(this Type type)
        {
            var constructor = type.GetConstructor(EmptyArguments);
            if (constructor == null)
            {
                throw new System.Exception();
            }
            var newExp = Expression.New(constructor);
            return (Func<T>)Expression.Lambda(newExp).Compile();
        }

        public static Func<object> GetCreator(this Type type)
        {
            var constructor = type.GetConstructor(EmptyArguments);
            if (constructor == null)
            {
                throw new System.Exception();
            }
            var newExp = Expression.New(constructor);
            return (Func<object>)Expression.Lambda(newExp).Compile();
        }

        public static object Default(this Type type)
        {
            if (type.IsPrimitive)
            {
                return DefaultValue.GetOrAdd(type,
                    (key) => Expression.Lambda<Func<object>>(Expression.Default(key)).Compile()());
            }

            return null;
        }
    }
}
