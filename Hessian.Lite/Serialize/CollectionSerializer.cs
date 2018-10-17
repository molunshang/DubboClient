﻿using System.Collections;
using System.Collections.Generic;

namespace Hessian.Lite.Serialize
{
    public class CollectionSerializer : EnumerableSerializer
    {
        protected override bool WriteListBegin(object obj, Hessian2Writer writer)
        {
            var listType = obj.GetType();
            var collection = (ICollection)obj;
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
                        SerializeFactory.TryGetMapType(genericType.AssemblyQualifiedName, out var mapType)
                            ? mapType
                            : listType.AssemblyQualifiedName);
                }
            }
            else
            {
                writer.WriteListStart(collection.Count, collection is ArrayList ? null : listType.AssemblyQualifiedName);
            }
            return false;
        }
    }
}