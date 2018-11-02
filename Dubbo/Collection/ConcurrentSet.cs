using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Dubbo.Collection
{
    public class ConcurrentSet<T> : ISet<T>
    {
        private static readonly object SingleObj = new object();
        private readonly ConcurrentDictionary<T, object> _dictionary = new ConcurrentDictionary<T, object>();

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            ((ISet<T>)this).Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _dictionary.Keys.Except(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _dictionary.Keys.Intersect(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new System.NotImplementedException();
        }

        public bool Add(T item)
        {
            return _dictionary.TryAdd(item, SingleObj);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _dictionary.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _dictionary.TryRemove(item, out var ignore);
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;
    }
}