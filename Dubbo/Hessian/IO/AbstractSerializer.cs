using Hessian.IO.Log;

namespace Hessian.IO
{
    public abstract class AbstractSerializer : Serializer
    {
        protected internal static readonly Logger log = Logger.GetLogger(typeof(AbstractSerializer).FullName);

        public abstract void writeObject(object obj, AbstractHessianOutput @out);
    }

}