using System;

namespace Hessian.Lite.Deserialize
{
    public abstract class AbstractDeserializer : IHessianDeserializer
    {
        public Type Type { get; protected set; }

        public virtual object ReadList(Hessian2Reader reader, int length)
        {
            throw new NotSupportedException();
        }

        public virtual object ReadMap(Hessian2Reader reader)
        {
            throw new NotSupportedException();
        }

        public virtual object ReadObject(Hessian2Reader reader)
        {
            throw new NotSupportedException();
        }

        public virtual object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            throw new NotSupportedException();
        }
    }
}