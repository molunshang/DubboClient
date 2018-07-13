namespace Hessian.IO
{

	/// <summary>
	/// Looks up remote objects.  The default just returns a HessianRemote object.
	/// </summary>
	public class AbstractHessianResolver : HessianRemoteResolver
	{
	  /// <summary>
	  /// Looks up a proxy object.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object lookup(String type, String url) throws java.io.IOException
	  public virtual object lookup(string type, string url)
	  {
		return new HessianRemote(type, url);
	  }
	}

}