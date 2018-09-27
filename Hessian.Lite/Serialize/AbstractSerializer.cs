namespace Hessian.Lite.Serialize
{
    public abstract class AbstractSerializer : IHessianSerializer
    {
        protected static readonly object SyncRoot = new object();
        protected abstract void DoWrite(object obj, HessianWriter writer);
        public void WriteObject(object obj, HessianWriter writer)
        {
            if (writer.WriteRef(obj))
            {
                return;
            }
            DoWrite(obj, writer);
        }
    }
}