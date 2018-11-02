using System;

namespace Dubbo.Attribute
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DubboServiceAttribute : System.Attribute
    {
        public string TargetService { get; set; }
        public string Version { get; set; }
        public string Group { get; set; }
        public int Timeout { get; set; }
    }
}