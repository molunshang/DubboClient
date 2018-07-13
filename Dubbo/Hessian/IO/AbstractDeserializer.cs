using System;

namespace Hessian.IO
{

    /// <summary>
    /// Deserializing an object. 
    /// </summary>
    public abstract class AbstractDeserializer : Deserializer
    {
        public virtual Type Type
        {
            get
            {
                return typeof(object);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readObject(AbstractHessianInput in) throws java.io.IOException
        public virtual object readObject(AbstractHessianInput @in)
        {
            object obj = @in.readObject();

            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            string className = this.GetType().FullName;

            if (obj != null)
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                throw error(className + ": unexpected object " + obj.GetType().FullName + " (" + obj + ")");
            }
            else
            {
                throw error(className + ": unexpected null value");
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readList(AbstractHessianInput in, int length) throws java.io.IOException
        public virtual object readList(AbstractHessianInput @in, int length)
        {
            throw new System.NotSupportedException(this.ToString());
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readLengthList(AbstractHessianInput in, int length) throws java.io.IOException
        public virtual object readLengthList(AbstractHessianInput @in, int length)
        {
            throw new System.NotSupportedException(this.ToString());
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readMap(AbstractHessianInput in) throws java.io.IOException
        public virtual object readMap(AbstractHessianInput @in)
        {
            object obj = @in.readObject();

            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            string className = this.GetType().FullName;

            if (obj != null)
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                throw error(className + ": unexpected object " + obj.GetType().FullName + " (" + obj + ")");
            }
            else
            {
                throw error(className + ": unexpected null value");
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readObject(AbstractHessianInput in, String [] fieldNames) throws java.io.IOException
        public virtual object readObject(AbstractHessianInput @in, string[] fieldNames)
        {
            throw new System.NotSupportedException(this.ToString());
        }

        protected internal virtual HessianProtocolException error(string msg)
        {
            return new HessianProtocolException(msg);
        }

        protected internal virtual string codeName(int ch)
        {
            if (ch < 0)
            {
                return "end of file";
            }
            else
            {
                return "0x" + (ch & 0xff).ToString("x");
            }
        }
    }

}