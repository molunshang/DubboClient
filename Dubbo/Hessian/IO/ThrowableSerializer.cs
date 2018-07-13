using System;

namespace Hessian.IO
{

	/// <summary>
	/// Serializing an object for known object types.
	/// </summary>
	public class ThrowableSerializer : JavaSerializer
	{
	  public ThrowableSerializer(Type cl, ClassLoader loader) : base(cl, loader)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObject(Object obj, AbstractHessianOutput out) throws java.io.IOException
	  public override void writeObject(object obj, AbstractHessianOutput @out)
	  {
		Exception e = (Exception) obj;

		e.StackTrace;

		base.writeObject(obj, @out);
	  }
	}

}