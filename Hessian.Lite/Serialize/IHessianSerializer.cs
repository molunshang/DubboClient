namespace Hessian.Lite.Serialize
{
    public interface IHessianSerializer
    {
        void WriteObject(object obj, Hessian2Writer writer);
    }
}