using System;
using System.Collections;

namespace Hessian.Lite.Deserialize
{
    public class EnumeratorDeserializer : EnumerableDeserializer
    {

        public EnumeratorDeserializer(Type type) : base(type)
        {

        }
        public override object ReadObject(Hessian2Reader reader)
        {
            var result = base.ReadObject(reader);
            return ((IEnumerable)result)?.GetEnumerator();
        }

        public override object ReadList(Hessian2Reader reader, int length)
        {
            return ((IEnumerable)base.ReadList(reader, length)).GetEnumerator();
        }
    }
}