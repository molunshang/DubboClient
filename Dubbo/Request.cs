using System;
using System.Collections.Generic;

namespace Dubbo
{
    public class Request
    {
        public long RequestId { get; set; }
        public string MethodName { get; set; }
        public Type[] ParameterTypes { get; set; }
        public object[] Arguments { get; set; }
        public IDictionary<string, string> Attachments { get; set; }
    }
}
