using System.IO;

namespace Hessian.Lite.Serialize
{
    public class StreamSerializer : IHessianSerializer
    {
        public void WriteObject(object obj, Hessian2Writer writer)
        {
            writer.WriteStream((Stream)obj);
        }
    }
}