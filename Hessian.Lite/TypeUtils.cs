using System;
using System.Linq;
using System.Linq.Expressions;

namespace Hessian.Lite
{
    public static class TypeUtils
    {
        public static readonly Type[] EmptyArguments = new Type[0];
        public static bool IsSubType(this Type child, Type parent)
        {
            return parent.IsAssignableFrom(child) || child.GetInterfaces().Any(t => t == parent);
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
    }
}
