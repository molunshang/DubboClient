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
            reader.AddRef(result);
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = reader.ReadObject<T>();
            }
            return result;
        }

        private T[] ReadList(Hessian2Reader reader)
        {
            var list = new List<T>();
            reader.AddRef(list);
            while (!reader.HasEnd())
            {
                list.Add(reader.ReadObject<T>());
            }
            reader.ReadToEnd();
            return list.ToArray();
        }

        public override object ReadList(Hessian2Reader reader, int length)
        {
            return length >= 0 ? ReadArray(reader, length) : ReadList(reader);
        }
    }
}