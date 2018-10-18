using Hessian.Lite.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hessian.Lite.Deserialize
{
    public class ObjectDeserializer<T> : AbstractDeserializer
    {
        private readonly Dictionary<string, Action<T, Hessian2Reader>> _propertyInfos = new Dictionary<string, Action<T, Hessian2Reader>>();
        private readonly Func<T> _creator;
        public ObjectDeserializer()
        {
            var type = typeof(T);
            Type = type;
            _creator = type.GetCreator<T>();
            while (type != null && type != typeof(object))
            {
                var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField);
                if (properties.Length > 0)
                {
                    var readerType = typeof(Hessian2Reader);
                    var baseMethod = readerType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "ReadObject" && m.IsGenericMethod);
                    var methodCache = new Dictionary<Type, MethodCallExpression>();
                    var readerExp = Expression.Parameter(readerType, "reader");
                    var instanceExp = Expression.Parameter(type, "result");
                    foreach (var property in properties)
                    {
                        var attribute = property.GetCustomAttribute<NameAttribute>();
                        if (!methodCache.TryGetValue(property.PropertyType, out var callExp))
                        {
                            callExp = Expression.Call(readerExp, baseMethod.MakeGenericMethod(property.PropertyType));
                            methodCache.Add(property.PropertyType, callExp);
                        }
                        var setCallExp = Expression.Call(instanceExp, property.SetMethod, callExp);
                        var setter = Expression.Lambda<Action<T, Hessian2Reader>>(setCallExp, instanceExp, readerExp).Compile();
                        _propertyInfos.Add(attribute == null ? property.Name : attribute.TargetName, setter);
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
                var propertyName = reader.ReadString();
                if (_propertyInfos.TryGetValue(propertyName, out var setter))
                {
                    setter(result, reader);
                }
                else
                {
                    reader.ReadObject();
                }
            }

            reader.ReadToEnd();
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, string[] fieldNames)
        {
            var result = _creator();
            reader.AddRef(result);
            foreach (var propertyName in fieldNames)
            {
                if (_propertyInfos.TryGetValue(propertyName, out var setter))
                {
                    setter(result, reader);
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