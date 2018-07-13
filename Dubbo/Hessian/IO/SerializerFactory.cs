using System;
using System.Collections;
using System.Threading;

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
	/// Factory for returning serialization methods.
	/// </summary>
	public class SerializerFactory : AbstractSerializerFactory
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
	  private static readonly Logger log = Logger.getLogger(typeof(SerializerFactory).FullName);

	  private static Deserializer OBJECT_DESERIALIZER = new BasicDeserializer(BasicDeserializer.OBJECT);

	  private static Hashtable _staticSerializerMap;
	  private static Hashtable _staticDeserializerMap;
	  private static Hashtable _staticTypeMap;

	  private ClassLoader _loader;

	  protected internal Serializer _defaultSerializer;

	  // Additional factories
	  protected internal ArrayList _factories = new ArrayList();

	  protected internal CollectionSerializer _collectionSerializer;
	  protected internal MapSerializer _mapSerializer;

	  private Deserializer _hashMapDeserializer;
	  private Deserializer _arrayListDeserializer;
	  private Hashtable _cachedSerializerMap;
	  private Hashtable _cachedDeserializerMap;
	  private Hashtable _cachedTypeDeserializerMap;

	  private bool _isAllowNonSerializable;

	  public SerializerFactory() : this(Thread.CurrentThread.ContextClassLoader)
	  {
	  }

	  public SerializerFactory(ClassLoader loader)
	  {
		_loader = loader;
	  }

	  public virtual ClassLoader ClassLoader
	  {
		  get
		  {
			return _loader;
		  }
	  }

	  /// <summary>
	  /// Set true if the collection serializer should send the java type.
	  /// </summary>
	  public virtual bool SendCollectionType
	  {
		  set
		  {
			if (_collectionSerializer == null)
			{
			  _collectionSerializer = new CollectionSerializer();
			}
    
			_collectionSerializer.SendJavaType = value;
    
			if (_mapSerializer == null)
			{
			  _mapSerializer = new MapSerializer();
			}
    
			_mapSerializer.SendJavaType = value;
		  }
	  }

	  /// <summary>
	  /// Adds a factory.
	  /// </summary>
	  public virtual void addFactory(AbstractSerializerFactory factory)
	  {
		_factories.Add(factory);
	  }

	  /// <summary>
	  /// If true, non-serializable objects are allowed.
	  /// </summary>
	  public virtual bool AllowNonSerializable
	  {
		  set
		  {
			_isAllowNonSerializable = value;
		  }
		  get
		  {
			return _isAllowNonSerializable;
		  }
	  }


	  /// <summary>
	  /// Returns the serializer for a class.
	  /// </summary>
	  /// <param name="cl"> the class of the object that needs to be serialized.
	  /// </param>
	  /// <returns> a serializer object for the serialization. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Serializer getSerializer(Class cl) throws HessianProtocolException
	  public override Serializer getSerializer(Type cl)
	  {
		Serializer serializer;

		serializer = (Serializer) _staticSerializerMap[cl];
		if (serializer != null)
		{
		  return serializer;
		}

		if (_cachedSerializerMap != null)
		{
		  lock (_cachedSerializerMap)
		  {
		serializer = (Serializer) _cachedSerializerMap[cl];
		  }

		  if (serializer != null)
		  {
		return serializer;
		  }
		}

		for (int i = 0; serializer == null && _factories != null && i < _factories.Count; i++)
		{
		  AbstractSerializerFactory factory;

		  factory = (AbstractSerializerFactory) _factories[i];

		  serializer = factory.getSerializer(cl);
		}

		if (serializer != null)
		{
		}

		else if (JavaSerializer.getWriteReplace(cl) != null)
		{
		  serializer = new JavaSerializer(cl, _loader);
		}

		else if (cl.IsSubclassOf(typeof(HessianRemoteObject)))
		{
		  serializer = new RemoteSerializer();
		}

	//    else if (BurlapRemoteObject.class.isAssignableFrom(cl))
	//      serializer = new RemoteSerializer();

		else if (cl.IsSubclassOf(typeof(IDictionary)))
		{
		  if (_mapSerializer == null)
		  {
		_mapSerializer = new MapSerializer();
		  }

		  serializer = _mapSerializer;
		}
		else if (cl.IsSubclassOf(typeof(ICollection)))
		{
		  if (_collectionSerializer == null)
		  {
		_collectionSerializer = new CollectionSerializer();
		  }

		  serializer = _collectionSerializer;
		}

		else if (cl.IsArray)
		{
		  serializer = new ArraySerializer();
		}

		else if (cl.IsSubclassOf(typeof(Exception)))
		{
		  serializer = new ThrowableSerializer(cl, ClassLoader);
		}

		else if (cl.IsSubclassOf(typeof(System.IO.Stream)))
		{
		  serializer = new InputStreamSerializer();
		}

		else if (cl.IsSubclassOf(typeof(IEnumerator)))
		{
		  serializer = IteratorSerializer.create();
		}

		else if (cl.IsSubclassOf(typeof(System.Collections.IEnumerator)))
		{
		  serializer = EnumerationSerializer.create();
		}

		else if (cl.IsSubclassOf(typeof(DateTime)))
		{
		  serializer = CalendarSerializer.create();
		}

		else if (cl.IsSubclassOf(typeof(Locale)))
		{
		  serializer = LocaleSerializer.create();
		}

		else if (cl.IsSubclassOf(typeof(Enum)))
		{
		  serializer = new EnumSerializer(cl);
		}

		if (serializer == null)
		{
		  serializer = getDefaultSerializer(cl);
		}

		if (_cachedSerializerMap == null)
		{
		  _cachedSerializerMap = new Hashtable(8);
		}

		lock (_cachedSerializerMap)
		{
		  _cachedSerializerMap[cl] = serializer;
		}

		return serializer;
	  }

	  /// <summary>
	  /// Returns the default serializer for a class that isn't matched
	  /// directly.  Application can override this method to produce
	  /// bean-style serialization instead of field serialization.
	  /// </summary>
	  /// <param name="cl"> the class of the object that needs to be serialized.
	  /// </param>
	  /// <returns> a serializer object for the serialization. </returns>
	  protected internal virtual Serializer getDefaultSerializer(Type cl)
	  {
		if (_defaultSerializer != null)
		{
		  return _defaultSerializer;
		}

		if (!cl.IsSubclassOf(typeof(Serializable)) && !_isAllowNonSerializable)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new System.InvalidOperationException("Serialized class " + cl.FullName + " must implement java.io.Serializable");
		}

		return new JavaSerializer(cl, _loader);
	  }

	  /// <summary>
	  /// Returns the deserializer for a class.
	  /// </summary>
	  /// <param name="cl"> the class of the object that needs to be deserialized.
	  /// </param>
	  /// <returns> a deserializer object for the serialization. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Deserializer getDeserializer(Class cl) throws HessianProtocolException
	  public override Deserializer getDeserializer(Type cl)
	  {
		Deserializer deserializer;

		deserializer = (Deserializer) _staticDeserializerMap[cl];
		if (deserializer != null)
		{
		  return deserializer;
		}

		if (_cachedDeserializerMap != null)
		{
		  lock (_cachedDeserializerMap)
		  {
		deserializer = (Deserializer) _cachedDeserializerMap[cl];
		  }

		  if (deserializer != null)
		  {
		return deserializer;
		  }
		}


		for (int i = 0; deserializer == null && _factories != null && i < _factories.Count; i++)
		{
		  AbstractSerializerFactory factory;
		  factory = (AbstractSerializerFactory) _factories[i];

		  deserializer = factory.getDeserializer(cl);
		}

		if (deserializer != null)
		{
		}

		else if (cl.IsSubclassOf(typeof(ICollection)))
		{
		  deserializer = new CollectionDeserializer(cl);
		}

		else if (cl.IsSubclassOf(typeof(IDictionary)))
		{
		  deserializer = new MapDeserializer(cl);
		}

		else if (cl.IsInterface)
		{
		  deserializer = new ObjectDeserializer(cl);
		}

		else if (cl.IsArray)
		{
		  deserializer = new ArrayDeserializer(cl.GetElementType());
		}

		else if (cl.IsSubclassOf(typeof(System.Collections.IEnumerator)))
		{
		  deserializer = EnumerationDeserializer.create();
		}

		else if (cl.IsSubclassOf(typeof(Enum)))
		{
		  deserializer = new EnumDeserializer(cl);
		}

		else if (typeof(Type).Equals(cl))
		{
		  deserializer = new ClassDeserializer(_loader);
		}

		else
		{
		  deserializer = getDefaultDeserializer(cl);
		}

		if (_cachedDeserializerMap == null)
		{
		  _cachedDeserializerMap = new Hashtable(8);
		}

		lock (_cachedDeserializerMap)
		{
		  _cachedDeserializerMap[cl] = deserializer;
		}

		return deserializer;
	  }

	  /// <summary>
	  /// Returns the default serializer for a class that isn't matched
	  /// directly.  Application can override this method to produce
	  /// bean-style serialization instead of field serialization.
	  /// </summary>
	  /// <param name="cl"> the class of the object that needs to be serialized.
	  /// </param>
	  /// <returns> a serializer object for the serialization. </returns>
	  protected internal virtual Deserializer getDefaultDeserializer(Type cl)
	  {
		return new JavaDeserializer(cl);
	  }

	  /// <summary>
	  /// Reads the object as a list.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readList(AbstractHessianInput in, int length, String type) throws HessianProtocolException, IOException
	  public virtual object readList(AbstractHessianInput @in, int length, string type)
	  {
		Deserializer deserializer = getDeserializer(type);

		if (deserializer != null)
		{
		  return deserializer.readList(@in, length);
		}
		else
		{
		  return (new CollectionDeserializer(typeof(ArrayList))).readList(@in, length);
		}
	  }

	  /// <summary>
	  /// Reads the object as a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readMap(AbstractHessianInput in, String type) throws HessianProtocolException, IOException
	  public virtual object readMap(AbstractHessianInput @in, string type)
	  {
		Deserializer deserializer = getDeserializer(type);

		if (deserializer != null)
		{
		  return deserializer.readMap(@in);
		}
		else if (_hashMapDeserializer != null)
		{
		  return _hashMapDeserializer.readMap(@in);
		}
		else
		{
		  _hashMapDeserializer = new MapDeserializer(typeof(Hashtable));

		  return _hashMapDeserializer.readMap(@in);
		}
	  }

	  /// <summary>
	  /// Reads the object as a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in, String type, String [] fieldNames) throws HessianProtocolException, IOException
	  public virtual object readObject(AbstractHessianInput @in, string type, string[] fieldNames)
	  {
		Deserializer deserializer = getDeserializer(type);

		if (deserializer != null)
		{
		  return deserializer.readObject(@in, fieldNames);
		}
		else if (_hashMapDeserializer != null)
		{
		  return _hashMapDeserializer.readObject(@in, fieldNames);
		}
		else
		{
		  _hashMapDeserializer = new MapDeserializer(typeof(Hashtable));

		  return _hashMapDeserializer.readObject(@in, fieldNames);
		}
	  }

	  /// <summary>
	  /// Reads the object as a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Deserializer getObjectDeserializer(String type, Class cl) throws HessianProtocolException
	  public virtual Deserializer getObjectDeserializer(string type, Type cl)
	  {
		Deserializer reader = getObjectDeserializer(type);

		if (cl == null || cl.Equals(reader.Type) || cl.IsAssignableFrom(reader.Type) || reader.Type.IsSubclassOf(typeof(HessianHandle)))
		{
		  return reader;
		}

		if (log.isLoggable(Level.FINE))
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  log.fine("hessian: expected '" + cl.FullName + "' at '" + type + "' (" + reader.Type.FullName + ")");
		}

		return getDeserializer(cl);
	  }

	  /// <summary>
	  /// Reads the object as a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Deserializer getObjectDeserializer(String type) throws HessianProtocolException
	  public virtual Deserializer getObjectDeserializer(string type)
	  {
		Deserializer deserializer = getDeserializer(type);

		if (deserializer != null)
		{
		  return deserializer;
		}
		else if (_hashMapDeserializer != null)
		{
		  return _hashMapDeserializer;
		}
		else
		{
		  _hashMapDeserializer = new MapDeserializer(typeof(Hashtable));

		  return _hashMapDeserializer;
		}
	  }

	  /// <summary>
	  /// Reads the object as a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Deserializer getListDeserializer(String type, Class cl) throws HessianProtocolException
	  public virtual Deserializer getListDeserializer(string type, Type cl)
	  {
		Deserializer reader = getListDeserializer(type);

		if (cl == null || cl.Equals(reader.Type) || cl.IsAssignableFrom(reader.Type))
		{
		  return reader;
		}

		if (log.isLoggable(Level.FINE))
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  log.fine("hessian: expected '" + cl.FullName + "' at '" + type + "' (" + reader.Type.FullName + ")");
		}

		return getDeserializer(cl);
	  }

	  /// <summary>
	  /// Reads the object as a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Deserializer getListDeserializer(String type) throws HessianProtocolException
	  public virtual Deserializer getListDeserializer(string type)
	  {
		Deserializer deserializer = getDeserializer(type);

		if (deserializer != null)
		{
		  return deserializer;
		}
		else if (_arrayListDeserializer != null)
		{
		  return _arrayListDeserializer;
		}
		else
		{
		  _arrayListDeserializer = new CollectionDeserializer(typeof(ArrayList));

		  return _arrayListDeserializer;
		}
	  }

	  /// <summary>
	  /// Returns a deserializer based on a string type.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Deserializer getDeserializer(String type) throws HessianProtocolException
	  public virtual Deserializer getDeserializer(string type)
	  {
		if (string.ReferenceEquals(type, null) || type.Equals(""))
		{
		  return null;
		}

		Deserializer deserializer;

		if (_cachedTypeDeserializerMap != null)
		{
		  lock (_cachedTypeDeserializerMap)
		  {
		deserializer = (Deserializer) _cachedTypeDeserializerMap[type];
		  }

		  if (deserializer != null)
		  {
		return deserializer;
		  }
		}


		deserializer = (Deserializer) _staticTypeMap[type];
		if (deserializer != null)
		{
		  return deserializer;
		}

		if (type.StartsWith("[", StringComparison.Ordinal))
		{
		  Deserializer subDeserializer = getDeserializer(type.Substring(1));

		  if (subDeserializer != null)
		  {
			deserializer = new ArrayDeserializer(subDeserializer.Type);
		  }
		  else
		  {
			deserializer = new ArrayDeserializer(typeof(object));
		  }
		}
		else
		{
		  try
		  {
		Type cl = Type.GetType(type, false, _loader);
		deserializer = getDeserializer(cl);
		  }
		  catch (Exception e)
		  {
		log.warning("Hessian/Burlap: '" + type + "' is an unknown class in " + _loader + ":\n" + e);

		log.log(Level.FINER, e.ToString(), e);
		  }
		}

		if (deserializer != null)
		{
		  if (_cachedTypeDeserializerMap == null)
		  {
		_cachedTypeDeserializerMap = new Hashtable(8);
		  }

		  lock (_cachedTypeDeserializerMap)
		  {
		_cachedTypeDeserializerMap[type] = deserializer;
		  }
		}

		return deserializer;
	  }

	  private static void addBasic(Type cl, string typeName, int type)
	  {
		_staticSerializerMap[cl] = new BasicSerializer(type);

		Deserializer deserializer = new BasicDeserializer(type);
		_staticDeserializerMap[cl] = deserializer;
		_staticTypeMap[typeName] = deserializer;
	  }

	  static SerializerFactory()
	  {
		_staticSerializerMap = new Hashtable();
		_staticDeserializerMap = new Hashtable();
		_staticTypeMap = new Hashtable();

		addBasic(typeof(void), "void", BasicSerializer.NULL);

		addBasic(typeof(Boolean), "boolean", BasicSerializer.BOOLEAN);
		addBasic(typeof(Byte), "byte", BasicSerializer.BYTE);
		addBasic(typeof(Short), "short", BasicSerializer.SHORT);
		addBasic(typeof(Integer), "int", BasicSerializer.INTEGER);
		addBasic(typeof(Long), "long", BasicSerializer.LONG);
		addBasic(typeof(Float), "float", BasicSerializer.FLOAT);
		addBasic(typeof(Double), "double", BasicSerializer.DOUBLE);
		addBasic(typeof(Character), "char", BasicSerializer.CHARACTER_OBJECT);
		addBasic(typeof(string), "string", BasicSerializer.STRING);
		addBasic(typeof(object), "object", BasicSerializer.OBJECT);
		addBasic(typeof(DateTime), "date", BasicSerializer.DATE);

		addBasic(typeof(bool), "boolean", BasicSerializer.BOOLEAN);
		addBasic(typeof(sbyte), "byte", BasicSerializer.BYTE);
		addBasic(typeof(short), "short", BasicSerializer.SHORT);
		addBasic(typeof(int), "int", BasicSerializer.INTEGER);
		addBasic(typeof(long), "long", BasicSerializer.LONG);
		addBasic(typeof(float), "float", BasicSerializer.FLOAT);
		addBasic(typeof(double), "double", BasicSerializer.DOUBLE);
		addBasic(typeof(char), "char", BasicSerializer.CHARACTER);

		addBasic(typeof(bool[]), "[boolean", BasicSerializer.BOOLEAN_ARRAY);
		addBasic(typeof(sbyte[]), "[byte", BasicSerializer.BYTE_ARRAY);
		addBasic(typeof(short[]), "[short", BasicSerializer.SHORT_ARRAY);
		addBasic(typeof(int[]), "[int", BasicSerializer.INTEGER_ARRAY);
		addBasic(typeof(long[]), "[long", BasicSerializer.LONG_ARRAY);
		addBasic(typeof(float[]), "[float", BasicSerializer.FLOAT_ARRAY);
		addBasic(typeof(double[]), "[double", BasicSerializer.DOUBLE_ARRAY);
		addBasic(typeof(char[]), "[char", BasicSerializer.CHARACTER_ARRAY);
		addBasic(typeof(string[]), "[string", BasicSerializer.STRING_ARRAY);
		addBasic(typeof(object[]), "[object", BasicSerializer.OBJECT_ARRAY);

		_staticSerializerMap[typeof(Type)] = new ClassSerializer();

		_staticDeserializerMap[typeof(Number)] = new BasicDeserializer(BasicSerializer.NUMBER);

		_staticSerializerMap[typeof(decimal)] = new StringValueSerializer();
		try
		{
		  _staticDeserializerMap[typeof(decimal)] = new StringValueDeserializer(typeof(decimal));
		  _staticDeserializerMap[typeof(System.Numerics.BigInteger)] = new BigIntegerDeserializer();
		}
		catch (Exception)
		{
		}

		_staticSerializerMap[typeof(File)] = new StringValueSerializer();
		try
		{
		  _staticDeserializerMap[typeof(File)] = new StringValueDeserializer(typeof(File));
		}
		catch (Exception)
		{
		}

		_staticSerializerMap[typeof(ObjectName)] = new StringValueSerializer();
		try
		{
		  _staticDeserializerMap[typeof(ObjectName)] = new StringValueDeserializer(typeof(ObjectName));
		}
		catch (Exception)
		{
		}

		_staticSerializerMap[typeof(java.sql.Date)] = new SqlDateSerializer();
		_staticSerializerMap[typeof(java.sql.Time)] = new SqlDateSerializer();
		_staticSerializerMap[typeof(java.sql.Timestamp)] = new SqlDateSerializer();

		_staticSerializerMap[typeof(System.IO.Stream)] = new InputStreamSerializer();
		_staticDeserializerMap[typeof(System.IO.Stream)] = new InputStreamDeserializer();

		try
		{
		  _staticDeserializerMap[typeof(java.sql.Date)] = new SqlDateDeserializer(typeof(java.sql.Date));
		  _staticDeserializerMap[typeof(java.sql.Time)] = new SqlDateDeserializer(typeof(java.sql.Time));
		  _staticDeserializerMap[typeof(java.sql.Timestamp)] = new SqlDateDeserializer(typeof(java.sql.Timestamp));
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}

		// hessian/3bb5
		try
		{
		  Type stackTrace = typeof(StackTraceElement);

		  _staticDeserializerMap[stackTrace] = new StackTraceElementDeserializer();
		}
		catch (Exception)
		{
		}
	  }
	}

}