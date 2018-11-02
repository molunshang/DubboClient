using System;

namespace Dubbo
{
    public class InvokeContext
    {
        public string Service { get; set; }
        public string Group { get; set; }
        public string Version { get; set; }
        public int Timeout { get; set; }
        public string Method { get; set; }
        public string ParameterTypeInfo { get; set; }
        public bool IsAsync { get; set; }
        public Type ReturnType { get; set; }
    }
}
