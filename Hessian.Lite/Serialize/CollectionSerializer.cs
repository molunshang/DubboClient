using System.Collections;
using System.Collections.Generic;

namespace Hessian.Lite.Serialize
{
    public class CollectionSerializer : EnumerableSerializer
    {
        protected override bool WriteListBegin(object obj, Hessian2Writer writer)
        {
            var collection = (ICollection)obj;
            var listType = obj.GetType();
            if (listType.IsGenericType)
            {
                var genericType = listType.GetGenericTypeDefinition();
                if (genericType == typeof(List<>))
                {
                    writer.WriteListStart(collection.Count, null);
                }
                else
                {
                    writer.WriteListStart(collection.Count,
                        SerializeFactory.TryGetMapType(genericType.FullName, out var mapType)
                            ? mapType
                            : listType.FullName);
                }
            }
            else
            {
                writer.WriteListStart(collection.Count, collection is ArrayList ? null : listType.FullName);
            }
            return false;
        }
    }
}