using System;
using System.Collections.Generic;

namespace Hessian.Lite.Deserialize
{
    public class ArrayDeserializer<T> : AbstractDeserializer
    {
        public ArrayDeserializer()
        {
            Type = typeof(T).MakeArrayType();
        }

        private T[] ReadArray(Hessian2Reader reader, int length)
        {
            var result = new T[length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = reader.ReadObject<T>();
            }

            reader.AddRef(result);
            return result;
        }

        private T[] ReadList(Hessian2Reader reader)
        {
            var list = new List<T>();
            while (!reader.HasEnd())
            {
                list.Add(reader.ReadObject<T>());
            }
            reader.ReadToEnd();
            var result = list.ToArray();
            reader.AddRef(result);
            return result;
        }

        public override object ReadList(Hessian2Reader reader, int length)
        {
            return length >= 0 ? ReadArray(reader, length) : ReadList(reader);
        }
    }
}