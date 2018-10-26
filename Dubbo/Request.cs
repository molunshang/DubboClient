using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dubbo
{
    public class Request
    {
        public long RequestId { get; set; }
        public bool IsTwoWay { get; set; }
        public bool IsEvent { get; set; }
        public string MethodName { get; set; }
        public Type[] ParameterTypes { get; set; }
        public string ParameterTypeInfo { get; set; }
        public object[] Arguments { get; set; }
        public IDictionary<string, string> Attachments { get; set; } //

        public string GetParameterTypeDesc()
        {
            if (ParameterTypes == null || ParameterTypes.Length <= 0)
            {
                return string.Empty;
            }

            var desc = new StringBuilder(64);
            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                var type = ParameterTypes[i];
                while (type.IsArray)
                {
                    desc.Append('[');
                    type = type.GetElementType();
                }

                if (type.IsPrimitive)
                {
                }

                // 		if( c.isPrimitive() )
                // 		{
                // 			String t = c.getName();
                // 			if( "void".equals(t) ) ret.append(JVM_VOID);
                // 			else if( "boolean".equals(t) ) ret.append(JVM_BOOLEAN);
                // 			else if( "byte".equals(t) ) ret.append(JVM_BYTE);
                // 			else if( "char".equals(t) ) ret.append(JVM_CHAR);
                // 			else if( "double".equals(t) ) ret.append(JVM_DOUBLE);
                // 			else if( "float".equals(t) ) ret.append(JVM_FLOAT);
                // 			else if( "int".equals(t) ) ret.append(JVM_INT);
                // 			else if( "long".equals(t) ) ret.append(JVM_LONG);
                // 			else if( "short".equals(t) ) ret.append(JVM_SHORT);
                // 		}
                // 		else
                // 		{
                // 			ret.append('L');
                // 			ret.append(c.getName().replace('.', '/'));
                // 			ret.append(';');
                // 		}
                // 		return ret.toString();
            }

            return desc.ToString();
        }
    }
}