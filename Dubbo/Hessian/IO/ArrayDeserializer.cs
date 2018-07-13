using System;
using System.Collections;

namespace Hessian.IO
{


	/// <summary>
	/// Deserializing a Java array
	/// </summary>
	public class ArrayDeserializer : AbstractListDeserializer
	{
	  private Type _componentType;
	  private Type _type;

	  public ArrayDeserializer(Type componentType)
	  {
		_componentType = componentType;

		if (_componentType != null)
		{
		  try
		  {
			_type = Array.CreateInstance(_componentType, 0).GetType();
		  }
		  catch (Exception)
		  {
		  }
		}

		if (_type == null)
		{
		  _type = typeof(object[]);
		}
	  }

	  public override Type Type
	  {
		  get
		  {
			return _type;
		  }
	  }

	  /// <summary>
	  /// Reads the array.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readList(AbstractHessianInput in, int length) throws java.io.IOException
	  public override object readList(AbstractHessianInput @in, int length)
	  {
		if (length >= 0)
		{
		  object[] data = createArray(length);

		  @in.addRef(data);

		  if (_componentType != null)
		  {
			for (int i = 0; i < data.Length; i++)
			{
			  data[i] = @in.readObject(_componentType);
			}
		  }
		  else
		  {
			for (int i = 0; i < data.Length; i++)
			{
			  data[i] = @in.readObject();
			}
		  }

		  @in.readListEnd();

		  return data;
		}
		else
		{
		  ArrayList list = new ArrayList();

		  @in.addRef(list);

		  if (_componentType != null)
		  {
			while (!@in.End)
			{
			  list.Add(@in.readObject(_componentType));
			}
		  }
		  else
		  {
			while (!@in.End)
			{
			  list.Add(@in.readObject());
			}
		  }

		  @in.readListEnd();

		  object[] data = createArray(list.Count);
		  for (int i = 0; i < data.Length; i++)
		  {
			data[i] = list[i];
		  }

		  return data;
		}
	  }

	  /// <summary>
	  /// Reads the array.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readLengthList(AbstractHessianInput in, int length) throws java.io.IOException
	  public override object readLengthList(AbstractHessianInput @in, int length)
	  {
		object[] data = createArray(length);

		@in.addRef(data);

		if (_componentType != null)
		{
		  for (int i = 0; i < data.Length; i++)
		  {
		data[i] = @in.readObject(_componentType);
		  }
		}
		else
		{
		  for (int i = 0; i < data.Length; i++)
		  {
		data[i] = @in.readObject();
		  }
		}

		return data;
	  }

	  protected internal virtual object [] createArray(int length)
	  {
		if (_componentType != null)
		{
		  return (object []) Array.CreateInstance(_componentType, length);
		}
		else
		{
		  return new object[length];
		}
	  }

	  public override string ToString()
	  {
		return "ArrayDeserializer[" + _componentType + "]";
	  }
	}

}