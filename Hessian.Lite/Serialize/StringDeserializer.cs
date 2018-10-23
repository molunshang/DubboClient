using System;

namespace Hessian.Lite.Serialize
{
    public class StringDeserializer : AbstractDeserializer
    {

        private readonly Func<string, object> convertFunc;
        public StringDeserializer(Type type, Func<string, object> convertFunc)
        {
            Type = type;
            this.convertFunc = convertFunc;
        }

        public override object ReadMap(Hessian2Reader reader)
        {
            string value = null;
            while (!reader.HasEnd())
            {
                var key = reader.ReadString();
                if (key == "value")
                {
                    value = reader.ReadString();
                }
                else
                {
                    reader.ReadObject();
                }
            }
            reader.ReadToEnd();
            var result = convertFunc(value);
            reader.AddRef(result);
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, ObjectDefinition definition)
        {
            string value = null;
            foreach (var name in definition.Fields)
            {
                if ("value" == (name))
                {
                    value = reader.ReadString();
                }
                else
                {
                    reader.ReadObject();
                }
            }
            var result = convertFunc(value);
            reader.AddRef(result);
            return result;
        }
    }
}