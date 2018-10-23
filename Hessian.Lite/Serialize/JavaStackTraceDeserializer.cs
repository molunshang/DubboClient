using Hessian.Lite.Exception;

namespace Hessian.Lite.Serialize
{
    public class JavaStackTraceDeserializer : AbstractDeserializer
    {
        public JavaStackTraceDeserializer()
        {
            Type = typeof(JavaStackTrace);
        }

        public override object ReadMap(Hessian2Reader reader)
        {
            var result = new JavaStackTrace();
            reader.AddRef(result);
            while (!reader.HasEnd())
            {
                var key = reader.ReadString();
                switch (key)
                {
                    case "declaringClass":
                        result.DeclaringClass = reader.ReadString();
                        break;
                    case "methodName":
                        result.MethodName = reader.ReadString();
                        break;
                    case "fileName":
                        result.FileName = reader.ReadString();
                        break;
                    case "lineNumber":
                        result.LineNumber = reader.ReadInt();
                        break;
                }
            }
            reader.ReadToEnd();
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, ObjectDefinition definition)
        {
            var result = new JavaStackTrace();
            reader.AddRef(result);
            foreach (var key in definition.Fields)
            {
                switch (key)
                {
                    case "declaringClass":
                        result.DeclaringClass = reader.ReadString();
                        break;
                    case "methodName":
                        result.MethodName = reader.ReadString();
                        break;
                    case "fileName":
                        result.FileName = reader.ReadString();
                        break;
                    case "lineNumber":
                        result.LineNumber = reader.ReadInt();
                        break;
                }
            }
            return result;
        }
    }
}