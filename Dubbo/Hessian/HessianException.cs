using System;

namespace com.alibaba.com.caucho.hessian
{

	/// <summary>
	/// Base runtime exception for Hessian exceptions.
	/// </summary>
	public class HessianException : Exception
	{
		/// 
		private const long serialVersionUID = 4053135510253585150L;

		/// <summary>
		/// Zero-arg constructor.
		/// </summary>
		public HessianException()
		{
		}

		/// <summary>
		/// Create the exception.
		/// </summary>
		public HessianException(string message) : base(message)
		{
		}

		/// <summary>
		/// Create the exception.
		/// </summary>
		public HessianException(string message, Exception rootCause) : base(message, rootCause)
		{
		}		
	}

}