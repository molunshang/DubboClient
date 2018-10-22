using Hessian.Lite.Exception;

namespace Hessian.Lite.Deserialize
{
    public class JavaExceptionDeserializer : AbstractDeserializer
    {
        public JavaExceptionDeserializer()
        {
            Type = typeof(JavaException);
        }

        public override object ReadMap(Hessian2Reader reader)
        {
            var result = new JavaException();
            reader.AddRef(result);
            string msg = null;
            JavaStackTrace[] traces = null;
            JavaException[] suppressedExceptions = null;
            JavaException innerException = null;
            while (!reader.HasEnd())
            {
                var key = reader.ReadString();
                switch (key)
                {
                    case "detailMessage":
                        msg = reader.ReadString();
                        break;
                    case "stackTrace":
                        traces = reader.ReadObject<JavaStackTrace[]>();
                        break;
                    case "suppressedExceptions":
                        suppressedExceptions = reader.ReadObject<JavaException[]>();
                        break;
                    case "cause":
                        innerException = reader.ReadObject<JavaException>();
                        break;
                    default:
                        result.Data.Add(key, reader.ReadObject());
                        break;
                }
            }
            reader.ReadToEnd();
            result.ResumeException(string.Empty, msg, traces, suppressedExceptions, innerException);
            return result;
        }

        public override object ReadObject(Hessian2Reader reader, ObjectDefinition definition)
        {
            var result = new JavaException();
            reader.AddRef(result);
            string msg = null;
            JavaStackTrace[] traces = null;
            JavaException[] suppressedExceptions = null;
            JavaException innerException = null;
            foreach (var key in definition.Fields)
            {
                switch (key)
                {
                    case "detailMessage":
                        msg = reader.ReadString();
                        break;
                    case "stackTrace":
                        traces = reader.ReadObject<JavaStackTrace[]>();
                        break;
                    case "suppressedExceptions":
                        suppressedExceptions = reader.ReadObject<JavaException[]>();
                        break;
                    case "cause":
                        innerException = reader.ReadObject<JavaException>();
                        break;
                    default:
                        var info = reader.ReadObject();
                        if (info != result)
                        {
                            result.Data.Add(key, reader.ReadObject());
                        }
                        break;
                }
            }

            result.ResumeException(definition.Type, msg, traces, suppressedExceptions, innerException == result ? null : innerException);
            return result;
        }
    }
}