using Hessian.Lite.Attribute;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hessian.Lite.Serialize
{
    public class CSharpSerializer : AbstractSerializer
    {
        private readonly string _typeName;
        private readonly FieldInfo[] _fieldInfos;
        public CSharpSerializer(Type type)
        {
            var nameAttr = type.GetCustomAttribute<NameAttribute>();
            _typeName = nameAttr != null ? nameAttr.TargetName : type.AssemblyQualifiedName;
            _fieldInfos = GetFieldInfos(type);
        }

        private static FieldInfo[] GetFieldInfos(Type type)
        {
            var fieldList = new List<FieldInfo>();
            while (type != null && type != typeof(object))
            {
                var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
                if (fields.Length > 0)
                {
                    fieldList.AddRange(fields);
                }
                type = type.BaseType;
            }

            return fieldList.ToArray();
        }

        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            if (!writer.WriteObjectHeader(_typeName))
            {
                writer.WriteInt(_fieldInfos.Length);
                foreach (var field in _fieldInfos)
                {
                    writer.WriteString(field.Name);
                }

                writer.WriteObjectHeader(_typeName);
            }
            foreach (var fieldInfo in _fieldInfos)
            {
                writer.WriteObject(fieldInfo.GetValue(obj));
            }
        }
    }
}