using System;

namespace Dubbo.Attribute
{
    [AttributeUsage(AttributeTargets.All)]
    public class DubboTypeAttribute : System.Attribute
    {
        public string TargetType { get; set; }
    }
}