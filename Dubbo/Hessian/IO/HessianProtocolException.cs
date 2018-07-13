using System;
using System.IO;

namespace Hessian.IO
{

	/// <summary>
	/// Exception for faults when the fault doesn't return a java exception.
	/// This exception is required for MicroHessianInput.
	/// </summary>
	public class HessianProtocolException : IOException
	{
	  private Exception rootCause;

	  /// <summary>
	  /// Zero-arg constructor.
	  /// </summary>
	  public HessianProtocolException()
	  {
	  }

	  /// <summary>
	  /// Create the exception.
	  /// </summary>
	  public HessianProtocolException(string message) : base(message)
	  {
	  }

	  /// <summary>
	  /// Create the exception.
	  /// </summary>
	  public HessianProtocolException(string message, Exception rootCause) : base(message)
	  {

		this.rootCause = rootCause;
	  }

	  /// <summary>
	  /// Create the exception.
	  /// </summary>
	  public HessianProtocolException(Exception rootCause) : base(rootCause.ToString())
	  {

		this.rootCause = rootCause;
	  }

	  /// <summary>
	  /// Returns the underlying cause.
	  /// </summary>
	  public virtual Exception RootCause
	  {
		  get
		  {
			return rootCause;
		  }
	  }

	  /// <summary>
	  /// Returns the underlying cause.
	  /// </summary>
	  public virtual Exception Cause
	  {
		  get
		  {
			return RootCause;
		  }
	  }
	}

}