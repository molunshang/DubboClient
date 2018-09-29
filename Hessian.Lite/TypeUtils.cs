using System;
using System.Linq;

namespace Hessian.Lite
{
    public static class TypeUtils
    {
        public static bool IsSubType(this Type child, Type parent)
        {
            return parent.IsAssignableFrom(child) || child.GetInterfaces().Any(t => t == parent);
        }
    }
}
