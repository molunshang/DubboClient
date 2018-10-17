using System;

namespace Hessian.Lite.Serialize
{
    public class EnumSerializer : AbstractSerializer
    {
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            var type = obj.GetType();
            var name = Enum.GetName(type, obj);
            if (!writer.WriteObjectHeader(type.AssemblyQualifiedName))
            {
                writer.WriteInt(1);
                writer.WriteString("name");
                writer.WriteObjectHeader(type.AssemblyQualifiedName);
            }
            writer.WriteString(name);
        }
    }
}