using System;
using System.Collections.Generic;
using System.Threading;

namespace Dubbo
{
    public class Request
    {
        private static long _requestId;
        public Request()
        {
            RequestId = Interlocked.Increment(ref _requestId);
        }

        public long RequestId { get; }
        public bool IsTwoWay { get; set; }
        public bool IsEvent { get; set; }
        public string MethodName { get; set; }
        public string ParameterTypeInfo { get; set; }
        public object[] Arguments { get; set; }
        public IDictionary<string, string> Attachments { get; set; } //
    }
}