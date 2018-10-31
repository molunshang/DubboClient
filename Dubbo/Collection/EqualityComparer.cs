using System;
using System.Collections.Generic;

namespace Dubbo.Collection
{
    public class EqualityComparer
    {
        private class EqualityComparerWrapper<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equalsFunc;
            private readonly Func<T, int> _hashCodeFunc;
            public EqualityComparerWrapper(Func<T, T, bool> equalsFunc, Func<T, int> hashCodeFunc)
            {
                this._equalsFunc = equalsFunc;
                this._hashCodeFunc = hashCodeFunc;
            }

            public bool Equals(T x, T y)
            {
                return _equalsFunc(x, y);
            }

            public int GetHashCode(T obj)
            {
                return _hashCodeFunc(obj);
            }
        }

        public static IEqualityComparer<T> CreateEqualityComparer<T>(Func<T, T, bool> equalsFunc, Func<T, int> hashCodeFunc)
        {
            return new EqualityComparerWrapper<T>(equalsFunc, hashCodeFunc);
        }
    }
}