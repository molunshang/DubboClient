using System;
using System.Collections.Generic;
using System.Reflection;
using Hessian.Lite.Util;

namespace Hessian.Lite.Serialize
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
            reader.AddRef(result);
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
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, ObjectDefinition definition)
        {
            var result = _creator();
            reader.AddRef(result);
            foreach (var field in definition.Fields)
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
            return result;
        }
    }
}