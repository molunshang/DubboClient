namespace Hessian.IO
{

	/// <summary>
	/// @author <a href="mailto:gang.lvg@alibaba-inc.com">kimi</a>
	/// </summary>
	public class BigIntegerDeserializer : JavaDeserializer
	{

		public BigIntegerDeserializer() : base(typeof(System.Numerics.BigInteger))
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected Object instantiate() throws Exception
		protected internal override object instantiate()
		{
			return System.Numerics.BigInteger.Parse("0");
		}
	}

}