using System.Collections;
using System.Collections.Generic;

namespace Hessian.Lite.Serialize
{
    public class CollectionSerializer : EnumerableSerializer
    {
        protected override bool WriteListBegin(object obj, HessianWriter writer)
        {
            var listType = obj.GetType();
            var collection = (ICollection)obj;
            if (collection is ArrayList || (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                writer.WriteListStart(collection.Count, null);
            }
            else
            {
                writer.WriteListStart(collection.Count, listType.FullName);
            }
            return false;
        }
    }
}