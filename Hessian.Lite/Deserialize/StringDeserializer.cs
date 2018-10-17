using System;

namespace Hessian.Lite.Deserialize
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

        public override object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            string value = null;
            foreach (var name in fieldNames)
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