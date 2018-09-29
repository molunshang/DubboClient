namespace Hessian.Lite.Serialize
{
    public abstract class AbstractSerializer : IHessianSerializer
    {
        protected abstract void DoWrite(object obj, Hessian2Writer writer);
        public bool SendGenericType { get; set; }

        protected AbstractSerializer()
        {
            SendGenericType = true;
        }
        public void WriteObject(object obj, Hessian2Writer writer)
        {
            if (writer.WriteRef(obj))
            {
                return;
            }
            DoWrite(obj, writer);
        }
    }
}