using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hessian.Lite.Deserialize
{
    public class CSharpDeserializer : AbstractDeserializer
    {
        private readonly Dictionary<string, FieldInfo> fieldSetters = new Dictionary<string, FieldInfo>();
        private readonly Func<object> _creator;
        public CSharpDeserializer(Type type)
        {
            Type = type;
            _creator = type.GetCreator();
            while (type != null && type != typeof(object))
            {
                var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
                if (fields.Length > 0)
                {
                    foreach (var field in fields)
                    {
                        fieldSetters.Add(field.Name, field);
                    }
                }
                type = type.BaseType;
            }
        }

        public override object ReadMap(Hessian2Reader reader)
        {
            var result = _creator();
            while (!reader.HasEnd())
            {
                var field = reader.ReadString();
                if (fieldSetters.TryGetValue(field, out var setter))
                {
                    setter.SetValue(result, reader.ReadObject(setter.FieldType));
                }
                else
                {
                    reader.ReadObject();
                }
            }
            reader.ReadToEnd();
            reader.AddRef(result);
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            var result = _creator();
            foreach (var field in fieldNames)
            {
                if (fieldSetters.TryGetValue(field, out var setter))
                {
                    setter.SetValue(result, reader.ReadObject(setter.FieldType));
                }
                else
                {
                    reader.ReadObject();
                }
            }
            reader.AddRef(result);
            return result;
        }
    }
}