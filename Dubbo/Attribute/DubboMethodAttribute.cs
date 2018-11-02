using System;

namespace Dubbo.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DubboMethodAttribute : System.Attribute
    {
        public string TargetMethod { get; set; }
        public int Timeout { get; set; }
    }
}