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
            if (nameAttr != null)
            {
                _typeName = nameAttr.TargetName;
            }
            else
            {
                if (type.IsGenericType)
                {
                    var targetType = type.GetGenericTypeDefinition();
                    _typeName = SerializeFactory.TryGetMapType(targetType.AssemblyQualifiedName, out var mapType) ? mapType : type.AssemblyQualifiedName;
                }
                else
                {
                    _typeName = type.Name;
                }
            }
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
            _fieldInfos = fieldList.ToArray();
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