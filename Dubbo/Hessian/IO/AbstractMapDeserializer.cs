using System;
using System.Collections;

namespace Hessian.IO
{


	/// <summary>
	/// Serializing an object for known object types.
	/// </summary>
	public class AbstractMapDeserializer : AbstractDeserializer
	{

	  public override Type Type
	  {
		  get
		  {
			return typeof(Hashtable);
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in) throws java.io.IOException
	  public override object readObject(AbstractHessianInput @in)
	  {
		object obj = @in.readObject();

		if (obj != null)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw error("expected map/object at " + obj.GetType().FullName + " (" + obj + ")");
		}
		else
		{
		  throw error("expected map/object at null");
		}
	  }
	}

}