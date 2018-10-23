using System;
using Hessian.Lite.Exception;

namespace Hessian.Lite.Serialize
{
    public class EnumDeserializer : AbstractDeserializer
    {
        public EnumDeserializer(Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException($"the type {type.FullName} is not a enum type", nameof(type));
            }

            Type = type;
        }

        public override object ReadMap(Hessian2Reader reader)
        {
            string val = null;
            while (!reader.HasEnd())
            {
                if ("name" == reader.ReadString())
                {
                    val = reader.ReadString();
                }
                else
                {
                    reader.ReadObject();
                }
            }
            reader.ReadToEnd();
            if (string.IsNullOrEmpty(val))
            {
                throw new HessianException($"the value is null or empty where read enum type {Type.FullName}");
            }
            var result = Enum.Parse(Type, val);
            reader.AddRef(result);
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, ObjectDefinition definition)
        {
            string val = null;
            foreach (var name in definition.Fields)
            {
                if ("name" == name)
                {
                    val = reader.ReadString();
                }
                else
                {
                    reader.ReadObject();
                }
            }
            if (string.IsNullOrEmpty(val))
            {
                throw new HessianException($"the value is null or empty where read enum type {Type.FullName}");
            }
            var result = Enum.Parse(Type, val);
            reader.AddRef(result);
            return result;
        }
    }
}