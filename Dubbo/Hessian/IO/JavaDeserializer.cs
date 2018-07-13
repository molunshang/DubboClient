using System;
using System.Collections;
using System.Reflection;

/*
 * Copyright (c) 2001-2008 Caucho Technology, Inc.  All rights reserved.
 *
 * The Apache Software License, Version 1.1
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in
 *    the documentation and/or other materials provided with the
 *    distribution.
 *
 * 3. The end-user documentation included with the redistribution, if
 *    any, must include the following acknowlegement:
 *       "This product includes software developed by the
 *        Caucho Technology (http://www.caucho.com/)."
 *    Alternately, this acknowlegement may appear in the software itself,
 *    if and wherever such third-party acknowlegements normally appear.
 *
 * 4. The names "Burlap", "Resin", and "Caucho" must not be used to
 *    endorse or promote products derived from this software without prior
 *    written permission. For written permission, please contact
 *    info@caucho.com.
 *
 * 5. Products derived from this software may not be called "Resin"
 *    nor may "Resin" appear in their names without prior written
 *    permission of Caucho Technology.
 *
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL CAUCHO TECHNOLOGY OR ITS CONTRIBUTORS
 * BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
 * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN
 * IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * @author Scott Ferguson
 */

namespace Hessian.IO
{


	/// <summary>
	/// Serializing an object for known object types.
	/// </summary>
	public class JavaDeserializer : AbstractMapDeserializer
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
	  private static readonly Logger log = Logger.getLogger(typeof(JavaDeserializer).FullName);

	  private Type _type;
	  private Hashtable _fieldMap;
	  private Method _readResolve;
	  private Constructor _constructor;
	  private object[] _constructorArgs;

	  public JavaDeserializer(Type cl)
	  {
		_type = cl;
		_fieldMap = getFieldMap(cl);

		_readResolve = getReadResolve(cl);

		if (_readResolve != null)
		{
		  _readResolve.Accessible = true;
		}

		Constructor[] constructors = cl.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		long bestCost = long.MaxValue;

		for (int i = 0; i < constructors.Length; i++)
		{
		  Type[] param = constructors[i].ParameterTypes;
		  long cost = 0;

		  for (int j = 0; j < param.Length; j++)
		  {
		cost = 4 * cost;

		if (typeof(object).Equals(param[j]))
		{
		  cost += 1;
		}
		else if (typeof(string).Equals(param[j]))
		{
		  cost += 2;
		}
		else if (typeof(int).Equals(param[j]))
		{
		  cost += 3;
		}
		else if (typeof(long).Equals(param[j]))
		{
		  cost += 4;
		}
		else if (param[j].IsPrimitive)
		{
		  cost += 5;
		}
		else
		{
		  cost += 6;
		}
		  }

		  if (cost < 0 || cost > (1 << 48))
		  {
		cost = 1 << 48;
		  }

		  cost += (long) param.Length << 48;

		  if (cost < bestCost)
		  {
			_constructor = constructors[i];
			bestCost = cost;
		  }
		}

		if (_constructor != null)
		{
		  _constructor.Accessible = true;
		  Type[] @params = _constructor.ParameterTypes;
		  _constructorArgs = new object[@params.Length];
		  for (int i = 0; i < @params.Length; i++)
		  {
			_constructorArgs[i] = getParamArg(@params[i]);
		  }
		}
	  }

	  public override Type Type
	  {
		  get
		  {
			return _type;
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readMap(AbstractHessianInput in) throws java.io.IOException
	  public override object readMap(AbstractHessianInput @in)
	  {
		try
		{
		  object obj = instantiate();

		  return readMap(@in, obj);
		}
		catch (IOException e)
		{
		  throw e;
		}
		catch (Exception e)
		{
		  throw e;
		}
		catch (Exception e)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new IOExceptionWrapper(_type.FullName + ":" + e.Message, e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in, String [] fieldNames) throws java.io.IOException
	  public override object readObject(AbstractHessianInput @in, string[] fieldNames)
	  {
		try
		{
		  object obj = instantiate();

		  return readObject(@in, obj, fieldNames);
		}
		catch (IOException e)
		{
		  throw e;
		}
		catch (Exception e)
		{
		  throw e;
		}
		catch (Exception e)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new IOExceptionWrapper(_type.FullName + ":" + e.Message, e);
		}
	  }

	  /// <summary>
	  /// Returns the readResolve method
	  /// </summary>
	  protected internal virtual Method getReadResolve(Type cl)
	  {
		for (; cl != null; cl = cl.BaseType)
		{
		  Method[] methods = cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

		  for (int i = 0; i < methods.Length; i++)
		  {
		Method method = methods[i];

		if (method.Name.Equals("readResolve") && method.ParameterTypes.length == 0)
		{
		  return method;
		}
		  }
		}

		return null;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readMap(AbstractHessianInput in, Object obj) throws java.io.IOException
	  public virtual object readMap(AbstractHessianInput @in, object obj)
	  {
		try
		{
		  int @ref = @in.addRef(obj);

		  while (!@in.End)
		  {
			object key = @in.readObject();

			FieldDeserializer deser = (FieldDeserializer) _fieldMap[key];

			if (deser != null)
			{
		  deser.deserialize(@in, obj);
			}
			else
			{
			  @in.readObject();
			}
		  }

		  @in.readMapEnd();

		  object resolve = resolve(obj);

		  if (obj != resolve)
		  {
		@in.setRef(@ref, resolve);
		  }

		  return resolve;
		}
		catch (IOException e)
		{
		  throw e;
		}
		catch (Exception e)
		{
		  throw new IOExceptionWrapper(e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in, Object obj, String [] fieldNames) throws java.io.IOException
	  public virtual object readObject(AbstractHessianInput @in, object obj, string[] fieldNames)
	  {
		try
		{
		  int @ref = @in.addRef(obj);

		  for (int i = 0; i < fieldNames.Length; i++)
		  {
			string name = fieldNames[i];

			FieldDeserializer deser = (FieldDeserializer) _fieldMap[name];

			if (deser != null)
			{
		  deser.deserialize(@in, obj);
			}
			else
			{
			  @in.readObject();
			}
		  }

		  object resolve = resolve(obj);

		  if (obj != resolve)
		  {
		@in.setRef(@ref, resolve);
		  }

		  return resolve;
		}
		catch (IOException e)
		{
		  throw e;
		}
		catch (Exception e)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new IOExceptionWrapper(obj.GetType().FullName + ":" + e, e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Object resolve(Object obj) throws Exception
	  private object resolve(object obj)
	  {
		// if there's a readResolve method, call it
		try
		{
		  if (_readResolve != null)
		  {
			return _readResolve.invoke(obj, new object[0]);
		  }
		}
		catch (InvocationTargetException e)
		{
		  if (e.TargetException != null)
		  {
		throw e;
		  }
		}

		return obj;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected Object instantiate() throws Exception
	  protected internal virtual object instantiate()
	  {
		try
		{
		  if (_constructor != null)
		  {
		return _constructor.newInstance(_constructorArgs);
		  }
		  else
		  {
		return _type.newInstance();
		  }
		}
		catch (Exception e)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new HessianProtocolException("'" + _type.FullName + "' could not be instantiated", e);
		}
	  }

	  /// <summary>
	  /// Creates a map of the classes fields.
	  /// </summary>
	  protected internal virtual Hashtable getFieldMap(Type cl)
	  {
		Hashtable fieldMap = new Hashtable();

		for (; cl != null; cl = cl.BaseType)
		{
		  Field[] fields = cl.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		  for (int i = 0; i < fields.Length; i++)
		  {
			Field field = fields[i];

			if (Modifier.isTransient(field.Modifiers) || Modifier.isStatic(field.Modifiers))
			{
			  continue;
			}
			else if (fieldMap[field.Name] != null)
			{
			  continue;
			}

			// XXX: could parameterize the handler to only deal with public
			try
			{
			  field.Accessible = true;
			}
			catch (Exception e)
			{
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
			}

		Type type = field.Type;
		FieldDeserializer deser;

		if (typeof(string).Equals(type))
		{
		  deser = new StringFieldDeserializer(field);
		}
		else if (typeof(sbyte).Equals(type))
		{
		  deser = new ByteFieldDeserializer(field);
		}
		else if (typeof(short).Equals(type))
		{
		  deser = new ShortFieldDeserializer(field);
		}
		else if (typeof(int).Equals(type))
		{
		  deser = new IntFieldDeserializer(field);
		}
		else if (typeof(long).Equals(type))
		{
		  deser = new LongFieldDeserializer(field);
		}
		else if (typeof(float).Equals(type))
		{
		  deser = new FloatFieldDeserializer(field);
		}
		else if (typeof(double).Equals(type))
		{
		  deser = new DoubleFieldDeserializer(field);
		}
		else if (typeof(bool).Equals(type))
		{
		  deser = new BooleanFieldDeserializer(field);
		}
		else if (typeof(java.sql.Date).Equals(type))
		{
		  deser = new SqlDateFieldDeserializer(field);
		}
		else if (typeof(java.sql.Timestamp).Equals(type))
		{
		  deser = new SqlTimestampFieldDeserializer(field);
		}
		else if (typeof(java.sql.Time).Equals(type))
		{
		  deser = new SqlTimeFieldDeserializer(field);
		}
		else
		{
		  deser = new ObjectFieldDeserializer(field);
		}

			fieldMap[field.Name] = deser;
		  }
		}

		return fieldMap;
	  }

	  /// <summary>
	  /// Creates a map of the classes fields.
	  /// </summary>
	  protected internal static object getParamArg(Type cl)
	  {
		if (!cl.IsPrimitive)
		{
		  return null;
		}
		else if (typeof(bool).Equals(cl))
		{
		  return false;
		}
		else if (typeof(sbyte).Equals(cl))
		{
		  return new sbyte?((sbyte) 0);
		}
		else if (typeof(short).Equals(cl))
		{
		  return new short?((short) 0);
		}
		else if (typeof(char).Equals(cl))
		{
		  return new char?((char) 0);
		}
		else if (typeof(int).Equals(cl))
		{
		  return Convert.ToInt32(0);
		}
		else if (typeof(long).Equals(cl))
		{
		  return Convert.ToInt64(0);
		}
		else if (typeof(float).Equals(cl))
		{
		  return Convert.ToSingle(0);
		}
		else if (typeof(double).Equals(cl))
		{
		  return Convert.ToDouble(0);
		}
		else
		{
		  throw new System.NotSupportedException();
		}
	  }

	  internal abstract class FieldDeserializer
	  {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException;
		internal abstract void deserialize(AbstractHessianInput @in, object obj);
	  }

	  internal class ObjectFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal ObjectFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  object value = null;

		  try
		  {
		value = @in.readObject(_field.Type);

		_field.set(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class BooleanFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal BooleanFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  bool value = false;

		  try
		  {
		value = @in.readBoolean();

		_field.setBoolean(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class ByteFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal ByteFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  int value = 0;

		  try
		  {
		value = @in.readInt();

		_field.setByte(obj, (sbyte) value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class ShortFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal ShortFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  int value = 0;

		  try
		  {
		value = @in.readInt();

		_field.setShort(obj, (short) value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class IntFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal IntFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  int value = 0;

		  try
		  {
		value = @in.readInt();

		_field.setInt(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class LongFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal LongFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  long value = 0;

		  try
		  {
		value = @in.readLong();

		_field.setLong(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class FloatFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal FloatFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  double value = 0;

		  try
		  {
		value = @in.readDouble();

		_field.setFloat(obj, (float) value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class DoubleFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal DoubleFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  double value = 0;

		  try
		  {
		value = @in.readDouble();

		_field.setDouble(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class StringFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal StringFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  string value = null;

		  try
		  {
		value = @in.readString();

		_field.set(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class SqlDateFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal SqlDateFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  java.sql.Date value = null;

		  try
		  {
			DateTime date = (DateTime) @in.readObject();
			value = new java.sql.Date(date.Ticks);

			_field.set(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class SqlTimestampFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal SqlTimestampFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  java.sql.Timestamp value = null;

		  try
		  {
			DateTime date = (DateTime) @in.readObject();
			if (date != null)
			{
				value = new java.sql.Timestamp(date.Ticks);
			}

			_field.set(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

	  internal class SqlTimeFieldDeserializer : FieldDeserializer
	  {
		internal readonly Field _field;

		internal SqlTimeFieldDeserializer(Field field)
		{
		  _field = field;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize(AbstractHessianInput in, Object obj) throws java.io.IOException
		internal override void deserialize(AbstractHessianInput @in, object obj)
		{
		  java.sql.Time value = null;

		  try
		  {
			DateTime date = (DateTime) @in.readObject();
			value = new java.sql.Time(date.Ticks);

			_field.set(obj, value);
		  }
		  catch (Exception e)
		  {
			logDeserializeError(_field, obj, value, e);
		  }
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void logDeserializeError(Field field, Object obj, Object value, Throwable e) throws java.io.IOException
	  internal static void logDeserializeError(Field field, object obj, object value, Exception e)
	  {
		string fieldName = (field.DeclaringClass.Name + "." + field.Name);

		if (e is HessianFieldException)
		{
		  throw (HessianFieldException) e;
		}
		else if (e is IOException)
		{
		  throw new HessianFieldException(fieldName + ": " + e.Message, e);
		}

		if (value != null)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new HessianFieldException(fieldName + ": " + value.GetType().FullName + " (" + value + ")" + " cannot be assigned to '" + field.Type.Name + "'", e);
		}
		else
		{
		   throw new HessianFieldException(fieldName + ": " + field.Type.Name + " cannot be assigned from null", e);
		}
	  }
	}

}