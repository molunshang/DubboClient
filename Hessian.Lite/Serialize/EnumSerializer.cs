using Hessian.Lite.Attribute;
using System;
using System.Reflection;

namespace Hessian.Lite.Serialize
{
    public class EnumSerializer : AbstractSerializer
    {
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            var type = obj.GetType();
            var name = Enum.GetName(type, obj);
            var nameAttr = type.GetCustomAttribute<NameAttribute>();
            if (!writer.WriteObjectHeader(nameAttr == null ? type.AssemblyQualifiedName : nameAttr.TargetName))
            {
                writer.WriteInt(1);
                writer.WriteString("name");
                writer.WriteObjectHeader(type.AssemblyQualifiedName);
            }
            writer.WriteString(name);
        }
    }
}