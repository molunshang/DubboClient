using System;

namespace Hessian.IO
{

    /// <summary>
    /// Serializing a Java array.
    /// </summary>
    public class ArraySerializer : AbstractSerializer
    {
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void writeObject(Object obj, AbstractHessianOutput out) throws java.io.IOException
        public override void writeObject(object obj, AbstractHessianOutput @out)
        {
            if (@out.addRef(obj))
            {
                return;
            }

            object[] array = (object[])obj;

            bool hasEnd = @out.writeListBegin(array.Length, getArrayType(obj.GetType()));

            for (int i = 0; i < array.Length; i++)
            {
                @out.writeObject(array[i]);
            }

            if (hasEnd)
            {
                @out.writeListEnd();
            }
        }

        /// <summary>
        /// Returns the &lt;type> name for a &lt;list>.
        /// </summary>
        private string getArrayType(Type cl)
        {
            if (cl.IsArray)
            {
                return '[' + getArrayType(cl.GetElementType());
            }

            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            string name = cl.FullName;

            if (name.Equals("java.lang.String"))
            {
                return "string";
            }
            else if (name.Equals("java.lang.Object"))
            {
                return "object";
            }
            else if (name.Equals("java.util.Date"))
            {
                return "date";
            }
            else
            {
                return name;
            }
        }
    }

}