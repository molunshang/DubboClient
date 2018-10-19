using System.Collections;
using System.Collections.Generic;

namespace Hessian.Lite.Serialize
{
    public class MapSerializer : AbstractSerializer
    {
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            var type = obj.GetType();
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                writer.WriteMapBegin(genericType == typeof(Dictionary<,>)
                    ? null
                    : SerializeFactory.GetMapType(genericType.AssemblyQualifiedName));
            }
            else
            {
                writer.WriteMapBegin(obj is Hashtable ? null : type.AssemblyQualifiedName);
            }

            var dic = (IDictionary)obj;
            foreach (DictionaryEntry kv in dic)
            {
                writer.WriteObject(kv.Key);
                writer.WriteObject(kv.Value);
            }
            writer.WriteMapEnd();
        }
    }
}