using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace Hessian.Lite.Deserialize
{
    public class MapDeserializer : AbstractDeserializer
    {
        public override Type Type { get; }

        public MapDeserializer(Type type)
        {
            Type = type;
        }

        //private static ICollection<TItem> ReadGenericList<T, TItem>(Hessian2Reader reader, int length) where T : ICollection<TItem>, new()
        //{
        //    ICollection<TItem> collection = new T();
        //    if (length > 0)
        //    {
        //        for (int i = 0; i < length; i++)
        //        {
        //            collection.Add(reader.ReadObject<TItem>());
        //        }
        //    }
        //    else
        //    {
        //        while (!reader.HasEnd())
        //        {
        //            collection.Add(reader.ReadObject<TItem>());
        //        }
        //        reader.ReadToEnd();
        //    }

        //    return collection;
        //}

        private IDictionary ReadObjectList(Hessian2Reader reader, int length)
        {
            throw new NotImplementedException();
        }


        public override object ReadMap(Hessian2Reader reader)
        {
            throw new NotImplementedException();
        }

        public override object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            object result;
            if (Type.IsGenericType)
            {
                result = fieldNames.ToDictionary(name => name, name => reader.ReadObject());
            }
            else
            {
                var hashTable = new Hashtable();
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