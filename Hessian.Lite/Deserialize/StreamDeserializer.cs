using System.IO;

namespace Hessian.Lite.Deserialize
{
    public class StreamDeserializer : AbstractDeserializer
    {
        public StreamDeserializer()
        {
            Type = typeof(Stream);
        }

        public override object ReadObject(Hessian2Reader reader)
        {
            var bytes = reader.ReadBytes();
            return new MemoryStream(bytes, false);
        }
    }
}