namespace Hessian.Lite.Serialize
{
    public class StringSerializer : AbstractSerializer
    {
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            var type = obj.GetType();
            if (!writer.WriteObjectHeader(type.FullName))
            {
                writer.WriteInt(1);
                writer.WriteString("value");
                writer.WriteObjectHeader(type.FullName);
            }
            writer.WriteString(obj.ToString());
        }
    }
}