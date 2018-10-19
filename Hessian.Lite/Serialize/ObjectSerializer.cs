using Hessian.Lite.Attribute;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hessian.Lite.Serialize
{
    public class ObjectSerializer : AbstractSerializer
    {
        private readonly string _typeName;
        private readonly IDictionary<string, PropertyInfo> _propertyInfos;
        public ObjectSerializer(Type type)
        {
            var nameAttr = type.GetCustomAttribute<NameAttribute>();
            _typeName = nameAttr == null ? type.AssemblyQualifiedName : nameAttr.TargetName;
            _propertyInfos = GetPropertyInfos(type);
        }

        private static IDictionary<string, PropertyInfo> GetPropertyInfos(Type type)
        {
            var proInfos = new Dictionary<string, PropertyInfo>();
            while (type != null && type != typeof(object))
            {
                var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
                if (properties.Length > 0)
                {
                    foreach (var field in properties)
                    {
                        var attribute = field.GetCustomAttribute<NameAttribute>();
                        proInfos.Add(attribute == null ? field.Name : attribute.TargetName, field);
                    }
                }
                type = type.BaseType;
            }
            return proInfos;
        }

        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            if (!writer.WriteObjectHeader(_typeName))
            {
                writer.WriteInt(_propertyInfos.Count);
                foreach (var key in _propertyInfos.Keys)
                {
                    writer.WriteString(key);
                }
                writer.WriteObjectHeader(_typeName);
            }
            foreach (var fieldInfo in _propertyInfos.Values)
            {
                writer.WriteObject(fieldInfo.GetValue(obj));
            }
        }
    }
}