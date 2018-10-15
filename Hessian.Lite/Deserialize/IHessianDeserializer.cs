using System;

namespace Hessian.Lite.Deserialize
{
    public interface IHessianDeserializer
    {
        Type Type { get; }

        object ReadObject(Hessian2Reader reader);

        object ReadList(Hessian2Reader reader, int length);

        object ReadMap(Hessian2Reader reader);

        object ReadObject(Hessian2Reader reader, string[] fieldNames);
    }
}