using System.Collections;
using System.Collections.Generic;

namespace Hessian.Lite.Serialize
{
    public class ObjectSerializer : AbstractSerializer
    {
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            //var type = obj.GetType();
            //if (type.IsGenericType)
            //{
            //    var genericType = type.GetGenericTypeDefinition();
            //    if (genericType == typeof(Dictionary<,>))
            //    {
            //        writer.WriteMapBegin(null);
            //    }
            //    else
            //    {
            //        writer.WriteMapBegin(SendGenericType ? type.FullName : genericType.FullName);
            //    }
            //}
            //else
            //{
            //    writer.WriteMapBegin(obj is Hashtable ? null : type.FullName);
            //}

            //var dic = (IDictionary)obj;
            //foreach (DictionaryEntry kv in dic)
            //{
            //    writer.WriteObject(kv.Key);
            //    writer.WriteObject(kv.Value);
            //}
            //writer.WriteMapEnd();
        }
    }
}