using System.Collections;

namespace Hessian.Lite.Serialize
{
    public class EnumerableSerializer : AbstractSerializer
    {
        protected virtual bool WriteListBegin(object obj, HessianWriter writer)
        {
            return writer.WriteListStart(-1, null);
        }

        protected override void DoWrite(object obj, HessianWriter writer)
        {
            var hasEnd = WriteListBegin(obj, writer);
            var items = (IEnumerable)obj;
            foreach (var item in items)
            {
                writer.WriteObject(item);
            }
            if (hasEnd)
            {
                writer.WriteListEnd();
            }
        }
    }
}