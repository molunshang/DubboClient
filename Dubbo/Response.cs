using System;
using System.Collections.Generic;

namespace Dubbo
{
    public class Response
    {
        public const byte Null = 2;
        public const byte Value = 1;
        public const byte Exception = 0;
        public const byte NullWithAttachment = 5;
        public const byte ValueWithAttachment = 4;
        public const byte ExceptionWithAttachment = 3;
        private const byte OK = 20;


        public long ResponseId { get; set; }
        public bool IsEvent { get; set; }
        public bool IsTwoWay { get; set; }
        public byte Status { get; set; }
        public bool IsOk => Status == OK;
        public string ErrorMessage { get; set; }
        public object Result { get; set; }
        public Exception Error { get; set; }
        public IDictionary<string, string> Attachments { get; set; }
    }
}