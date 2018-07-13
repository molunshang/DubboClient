namespace Hessian.IO
{

	/// <summary>
	/// Deserializing a JDK 1.2 Collection.
	/// </summary>
	public class AbstractListDeserializer : AbstractDeserializer
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in) throws java.io.IOException
	  public override object readObject(AbstractHessianInput @in)
	  {
		object obj = @in.readObject();

		if (obj != null)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw error("expected list at " + obj.GetType().FullName + " (" + obj + ")");
		}
		else
		{
		  throw error("expected list at null");
		}
	  }
	}

}