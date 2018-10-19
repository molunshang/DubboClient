using Hessian.Lite.Attribute;

namespace Hessian.Lite.Exception
{
    public class JavaException : System.Exception
    {
        [Name("detailMessage")]
        public string DetailMessage { get; }
        [Name("SuppressedExceptions")]
        public object[] StackTrace { get; }
        [Name("SuppressedExceptions")]
        public System.Exception SuppressedExceptions { get; }

        public JavaException(string msg) : base(msg)
        {
            DetailMessage = msg;
        }
    }
}