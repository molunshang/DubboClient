namespace Hessian.Lite.Attribute
{
    public class NameAttribute : System.Attribute
    {
        public string TargetName { get; }
        public NameAttribute(string targetName)
        {
            TargetName = targetName;
        }
    }
}