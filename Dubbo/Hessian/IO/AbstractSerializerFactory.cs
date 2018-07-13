using System;

namespace Hessian.IO
{

	/// <summary>
	/// Factory for returning serialization methods.
	/// </summary>
	public abstract class AbstractSerializerFactory
	{
	  /// <summary>
	  /// Returns the serializer for a class.
	  /// </summary>
	  /// <param name="cl"> the class of the object that needs to be serialized.
	  /// </param>
	  /// <returns> a serializer object for the serialization. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Serializer getSerializer(Class cl) throws HessianProtocolException;
	  public abstract Serializer getSerializer(Type cl);

	  /// <summary>
	  /// Returns the deserializer for a class.
	  /// </summary>
	  /// <param name="cl"> the class of the object that needs to be deserialized.
	  /// </param>
	  /// <returns> a deserializer object for the serialization. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Deserializer getDeserializer(Class cl) throws HessianProtocolException;
	  public abstract Deserializer getDeserializer(Type cl);
	}

}