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
                if (genericType == typeof(Dictionary<,>))
                {
                    writer.WriteMapBegin(null);
                }
                else
                {
                    writer.WriteMapBegin(SerializeFactory.TryGetMapType(genericType.FullName, out var targetType)
                        ? targetType
                        : type.FullName);
                }
            }
            else
            {
                writer.WriteMapBegin(obj is Hashtable ? null : type.FullName);
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