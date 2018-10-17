namespace Hessian.Lite.Deserialize
{
    public class ObjectDefinition
    {
        public string Type { get; }
        public string[] Fields { get; }

        public ObjectDefinition(string type, string[] fields)
        {
            Type = type;
            Fields = fields;
        }
    }
}