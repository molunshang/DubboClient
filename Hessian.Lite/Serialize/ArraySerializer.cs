using System;

namespace Hessian.Lite.Serialize
{
    public class ArraySerializer : AbstractSerializer
    {
        private string GetArrayType(Type type)
        {
            if (type.IsArray)
            {
                return "[" + GetArrayType(type.GetElementType());
            }

            switch (type.FullName)
            {
                case "System.String":
                    return "string";
                case "System.Object":
                    return "object";
                case "System.DateTime":
                    return "date";
                case "System.Boolean":
                    return "boolean";
                case "System.Int16":
                    return "short";
                case "System.Int32":
                    return "int";
                case "System.Int64":
                    return "long";
                case "System.Single":
                    return "float";
                case "System.Double":
                    return "double";
            }
            return type.FullName;
        }
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            var array = (object[])obj;
            writer.WriteListStart(array.Length, GetArrayType(obj.GetType()));
            foreach (var item in array)
            {
                writer.WriteObject(item);
            }
        }
    }
}