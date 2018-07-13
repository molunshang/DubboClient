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
	public class JavaSerializer : AbstractSerializer
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
	  private new static readonly Logger log = Logger.getLogger(typeof(JavaSerializer).FullName);

	  private static object[] NULL_ARGS = new object[0];

	  private Field[] _fields;
	  private FieldSerializer[] _fieldSerializers;

	  private object _writeReplaceFactory;
	  private Method _writeReplace;

	  public JavaSerializer(Type cl, ClassLoader loader)
	  {
		introspectWriteReplace(cl, loader);

		if (_writeReplace != null)
		{
		  _writeReplace.Accessible = true;
		}

		ArrayList primitiveFields = new ArrayList();
		ArrayList compoundFields = new ArrayList();

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

		// XXX: could parameterize the handler to only deal with public
		field.Accessible = true;

		if (field.Type.Primitive || (field.Type.Name.StartsWith("java.lang.") && !field.Type.Equals(typeof(object))))
		{
		  primitiveFields.Add(field);
		}
		else
		{
		  compoundFields.Add(field);
		}
		  }
		}

		ArrayList fields = new ArrayList();
		fields.AddRange(primitiveFields);
		fields.AddRange(compoundFields);

		_fields = new Field[fields.Count];
		fields.toArray(_fields);

		_fieldSerializers = new FieldSerializer[_fields.Length];

		for (int i = 0; i < _fields.Length; i++)
		{
		  _fieldSerializers[i] = getFieldSerializer(_fields[i].Type);
		}
	  }

	  private void introspectWriteReplace(Type cl, ClassLoader loader)
	  {
		try
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  string className = cl.FullName + "HessianSerializer";

		  Type serializerClass = Type.GetType(className, false, loader);

		  object serializerObject = serializerClass.newInstance();

		  Method writeReplace = getWriteReplace(serializerClass, cl);

		  if (writeReplace != null)
		  {
		_writeReplaceFactory = serializerObject;
		_writeReplace = writeReplace;

		return;
		  }
		}
		catch (ClassNotFoundException)
		{
		}
		catch (Exception e)
		{
		  log.log(Level.FINER, e.ToString(), e);
		}

		_writeReplace = getWriteReplace(cl);
	  }

	  /// <summary>
	  /// Returns the writeReplace method
	  /// </summary>
	  protected internal static Method getWriteReplace(Type cl)
	  {
		for (; cl != null; cl = cl.BaseType)
		{
		  Method[] methods = cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

		  for (int i = 0; i < methods.Length; i++)
		  {
		Method method = methods[i];

		if (method.Name.Equals("writeReplace") && method.ParameterTypes.length == 0)
		{
		  return method;
		}
		  }
		}

		return null;
	  }

	  /// <summary>
	  /// Returns the writeReplace method
	  /// </summary>
	  protected internal virtual Method getWriteReplace(Type cl, Type param)
	  {
		for (; cl != null; cl = cl.BaseType)
		{
		  foreach (Method method in cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
		  {
		if (method.Name.Equals("writeReplace") && method.ParameterTypes.length == 1 && param.Equals(method.ParameterTypes[0]))
		{
		  return method;
		}
		  }
		}

		return null;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObject(Object obj, AbstractHessianOutput out) throws java.io.IOException
	  public override void writeObject(object obj, AbstractHessianOutput @out)
	  {
		if (@out.addRef(obj))
		{
		  return;
		}

		Type cl = obj.GetType();

		try
		{
		  if (_writeReplace != null)
		  {
		object repl;

		if (_writeReplaceFactory != null)
		{
		  repl = _writeReplace.invoke(_writeReplaceFactory, obj);
		}
		else
		{
		  repl = _writeReplace.invoke(obj);
		}

		@out.removeRef(obj);

		@out.writeObject(repl);

		@out.replaceRef(repl, obj);

		return;
		  }
		}
		catch (Exception e)
		{
		  throw e;
		}
		catch (Exception e)
		{
		  // log.log(Level.FINE, e.toString(), e);
		  throw new Exception(e);
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		int @ref = @out.writeObjectBegin(cl.FullName);

		if (@ref < -1)
		{
		  writeObject10(obj, @out);
		}
		else
		{
		  if (@ref == -1)
		  {
		writeDefinition20(@out);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		@out.writeObjectBegin(cl.FullName);
		  }

		  writeInstance(obj, @out);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeObject10(Object obj, AbstractHessianOutput out) throws java.io.IOException
	  private void writeObject10(object obj, AbstractHessianOutput @out)
	  {
		for (int i = 0; i < _fields.Length; i++)
		{
		  Field field = _fields[i];

		  @out.WriteString(field.Name);

		  _fieldSerializers[i].serialize(@out, obj, field);
		}

		@out.writeMapEnd();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeDefinition20(AbstractHessianOutput out) throws java.io.IOException
	  private void writeDefinition20(AbstractHessianOutput @out)
	  {
		@out.writeClassFieldLength(_fields.Length);

		for (int i = 0; i < _fields.Length; i++)
		{
		  Field field = _fields[i];

		  @out.WriteString(field.Name);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeInstance(Object obj, AbstractHessianOutput out) throws java.io.IOException
	  public virtual void writeInstance(object obj, AbstractHessianOutput @out)
	  {
		for (int i = 0; i < _fields.Length; i++)
		{
		  Field field = _fields[i];

		  _fieldSerializers[i].serialize(@out, obj, field);
		}
	  }

	  private static FieldSerializer getFieldSerializer(Type type)
	  {
		if (typeof(int).Equals(type) || typeof(sbyte).Equals(type) || typeof(short).Equals(type) || typeof(int).Equals(type))
		{
		  return IntFieldSerializer.SER;
		}
		else if (typeof(long).Equals(type))
		{
		  return LongFieldSerializer.SER;
		}
		else if (typeof(double).Equals(type) || typeof(float).Equals(type))
		{
		  return DoubleFieldSerializer.SER;
		}
		else if (typeof(bool).Equals(type))
		{
		  return BooleanFieldSerializer.SER;
		}
		else if (typeof(string).Equals(type))
		{
		  return StringFieldSerializer.SER;
		}
		else if (typeof(DateTime).Equals(type) || typeof(java.sql.Date).Equals(type) || typeof(java.sql.Timestamp).Equals(type) || typeof(java.sql.Time).Equals(type))
		{
		  return DateFieldSerializer.SER;
		}
		else
		{
		  return FieldSerializer.SER;
		}
	  }

	  internal class FieldSerializer
	  {
		internal static readonly FieldSerializer SER = new FieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal virtual void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  object value = null;

		  try
		  {
		value = field.get(obj);
		  }
		  catch (IllegalAccessException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  try
		  {
		@out.writeObject(value);
		  }
		  catch (Exception e)
		  {
		throw new Exception(e.Message + "\n Java field: " + field, e);
		  }
		  catch (IOException e)
		  {
		throw new IOExceptionWrapper(e.Message + "\n Java field: " + field, e);
		  }
		}
	  }

	  internal class BooleanFieldSerializer : FieldSerializer
	  {
		internal new static readonly FieldSerializer SER = new BooleanFieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal override void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  bool value = false;

		  try
		  {
		value = field.getBoolean(obj);
		  }
		  catch (IllegalAccessException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  @out.writeBoolean(value);
		}
	  }

	  internal class IntFieldSerializer : FieldSerializer
	  {
		internal new static readonly FieldSerializer SER = new IntFieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal override void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  int value = 0;

		  try
		  {
		value = field.getInt(obj);
		  }
		  catch (IllegalAccessException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  @out.writeInt(value);
		}
	  }

	  internal class LongFieldSerializer : FieldSerializer
	  {
		internal new static readonly FieldSerializer SER = new LongFieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal override void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  long value = 0;

		  try
		  {
		value = field.getLong(obj);
		  }
		  catch (IllegalAccessException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  @out.writeLong(value);
		}
	  }

	  internal class DoubleFieldSerializer : FieldSerializer
	  {
		internal new static readonly FieldSerializer SER = new DoubleFieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal override void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  double value = 0;

		  try
		  {
		value = field.getDouble(obj);
		  }
		  catch (IllegalAccessException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  @out.writeDouble(value);
		}
	  }

	  internal class StringFieldSerializer : FieldSerializer
	  {
		internal new static readonly FieldSerializer SER = new StringFieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal override void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  string value = null;

		  try
		  {
		value = (string) field.get(obj);
		  }
		  catch (IllegalAccessException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  @out.WriteString(value);
		}
	  }

	  internal class DateFieldSerializer : FieldSerializer
	  {
		internal new static readonly FieldSerializer SER = new DateFieldSerializer();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize(AbstractHessianOutput out, Object obj, Field field) throws java.io.IOException
		internal override void serialize(AbstractHessianOutput @out, object obj, Field field)
		{
		  DateTime value = null;

		  try
		  {
			value = (DateTime) field.get(obj);
		  }
		  catch (IllegalAccessException e)
		  {
			log.log(Level.FINE, e.ToString(), e);
		  }

		  if (value == null)
		  {
			@out.writeNull();
		  }
		  else
		  {
			@out.writeUTCDate(value.Ticks);
		  }
		}
	  }
	}

}