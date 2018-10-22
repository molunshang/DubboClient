namespace Hessian.Lite.Exception
{
    public class JavaStackTrace
    {
        public string DeclaringClass { get; set; }
        public string MethodName { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }

        public override string ToString()
        {
            return DeclaringClass + "." + MethodName + (LineNumber == -2 ? "(Native Method)" : (FileName != null && LineNumber >= 0 ? "(" + FileName + ":" + LineNumber + ")" : (FileName != null ? "(" + FileName + ")" : "(Unknown Source)")));
        }
    }
}