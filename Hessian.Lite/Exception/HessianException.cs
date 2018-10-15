namespace Hessian.Lite.Exception
{
    public class HessianException : System.Exception
    {
        public int Code { get; private set; }
        public HessianException(int code, string msg) : base(msg)
        {
            Code = code;
        }
    }
}