using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Hessian.Lite.Exception
{
    public class JavaException : System.Exception
    {
        private static readonly FieldInfo InnerExceptionField = typeof(System.Exception).GetField("_innerException", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly Hashtable _extendInfo = new Hashtable();
        private bool isFreeze = false;
        public string ExceptionType { get; private set; }
        public string DetailMessage { get; private set; }
        public JavaStackTrace[] StackTraces { get; private set; }
        public JavaException[] SuppressedExceptions { get; private set; }

        public override IDictionary Data => _extendInfo;

        public override string Message => DetailMessage;

        private static void AppendMessage(StringBuilder msg, string prefix, int indent, JavaException ex)
        {
            while (true)
            {
                for (var j = 0; j < indent; j++)
                {
                    msg.Append("\t");
                }
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    msg.Append(prefix);
                }

                msg.Append(ex.ExceptionType).Append(":").Append(ex.DetailMessage).Append(Environment.NewLine);
                if (ex.StackTraces != null && ex.StackTraces.Length > 0)
                {
                    foreach (var trace in ex.StackTraces)
                    {
                        for (var j = 0; j < indent; j++)
                        {
                            msg.Append("\t");
                        }
                        msg.Append(" at ").Append(trace).Append(Environment.NewLine);
                    }
                }
                if (ex.SuppressedExceptions != null && ex.SuppressedExceptions.Length > 0)
                {
                    foreach (var current in ex.SuppressedExceptions)
                    {
                        AppendMessage(msg, "Suppressed: ", indent + 1, current);
                    }
                }

                if (ex.InnerException == null)
                {
                    return;
                }

                prefix = "Caused by: ";
                ex = (JavaException)ex.InnerException;
            }
        }

        public override string ToString()
        {
            var msg = new StringBuilder();
            AppendMessage(msg, null, 0, this);
            return msg.ToString();
        }

        public void ResumeException(string exceptionType, string detailMessage, JavaStackTrace[] stackTraces, JavaException[] suppressedExceptions, JavaException innerException)
        {
            if (isFreeze)
            {
                throw new InvalidOperationException("the exception has been frozen");
            }
            ExceptionType = exceptionType;
            DetailMessage = detailMessage;
            StackTraces = stackTraces;
            SuppressedExceptions = suppressedExceptions;
            if (innerException != null)
            {
                InnerExceptionField.SetValue(this, innerException);
            }
            isFreeze = true;
        }
    }
}