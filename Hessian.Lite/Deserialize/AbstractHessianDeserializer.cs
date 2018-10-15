using System;

namespace Hessian.Lite.Deserialize
{
    public abstract class AbstractHessianDeserializer : IHessianDeserializer
    {
        public abstract Type Type { get; }

        public virtual object ReadList(Hessian2Reader reader, int length)
        {
            throw new NotImplementedException();
        }

        public virtual object ReadMap(Hessian2Reader reader)
        {
            throw new NotImplementedException();
        }

        public virtual object ReadObject(Hessian2Reader reader)
        {
            throw new NotImplementedException();
        }

        public virtual object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            throw new NotImplementedException();
        }
    }
}