namespace Hessian.IO
{


	/// <summary>
	/// Abstract output stream for Hessian requests.
	/// 
	/// <pre>
	/// OutputStream os = ...; // from http connection
	/// AbstractOutput out = new HessianSerializerOutput(os);
	/// String value;
	/// 
	/// out.startCall("hello");  // start hello call
	/// out.writeString("arg1"); // write a string argument
	/// out.completeCall();      // complete the call
	/// </pre>
	/// </summary>
	public abstract class AbstractHessianOutput
	{
	  // serializer factory
	  protected internal SerializerFactory _serializerFactory;

	  /// <summary>
	  /// Sets the serializer factory.
	  /// </summary>
	  public virtual SerializerFactory SerializerFactory
	  {
		  set
		  {
			_serializerFactory = value;
		  }
		  get
		  {
			return _serializerFactory;
		  }
	  }


	  /// <summary>
	  /// Gets the serializer factory.
	  /// </summary>
	  public SerializerFactory findSerializerFactory()
	  {
		SerializerFactory factory = _serializerFactory;

		if (factory == null)
		{
		  _serializerFactory = factory = new SerializerFactory();
		}

		return factory;
	  }

	  /// <summary>
	  /// Initialize the output with a new underlying stream.
	  /// </summary>
	  public virtual void init(System.IO.Stream os)
	  {
	  }

	  /// <summary>
	  /// Writes a complete method call.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(String method, Object [] args) throws java.io.IOException
	  public virtual void call(string method, object[] args)
	  {
		int length = args != null ? args.Length : 0;

		startCall(method, length);

		for (int i = 0; i < length; i++)
		{
		  writeObject(args[i]);
		}

		completeCall();
	  }

	  /// <summary>
	  /// Starts the method call:
	  /// 
	  /// <code><pre>
	  /// C
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void startCall() throws java.io.IOException;
	  public abstract void startCall();

	  /// <summary>
	  /// Starts the method call:
	  /// 
	  /// <code><pre>
	  /// C string int
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void startCall(String method, int length) throws java.io.IOException;
	  public abstract void startCall(string method, int length);

	  /// <summary>
	  /// For Hessian 2.0, use the Header envelope instead
	  /// 
	  /// @deprecated
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeHeader(String name) throws java.io.IOException
	  public virtual void writeHeader(string name)
	  {
		throw new System.NotSupportedException(this.GetType().Name);
	  }

	  /// <summary>
	  /// Writes the method tag.
	  /// 
	  /// <code><pre>
	  /// string
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeMethod(String method) throws java.io.IOException;
	  public abstract void writeMethod(string method);

	  /// <summary>
	  /// Completes the method call:
	  /// 
	  /// <code><pre>
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void completeCall() throws java.io.IOException;
	  public abstract void completeCall();

	  /// <summary>
	  /// Writes a boolean value to the stream.  The boolean will be written
	  /// with the following syntax:
	  /// 
	  /// <code><pre>
	  /// T
	  /// F
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the boolean value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeBoolean(boolean value) throws java.io.IOException;
	  public abstract void writeBoolean(bool value);

	  /// <summary>
	  /// Writes an integer value to the stream.  The integer will be written
	  /// with the following syntax:
	  /// 
	  /// <code><pre>
	  /// I b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the integer value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeInt(int value) throws java.io.IOException;
	  public abstract void writeInt(int value);

	  /// <summary>
	  /// Writes a long value to the stream.  The long will be written
	  /// with the following syntax:
	  /// 
	  /// <code><pre>
	  /// L b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the long value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeLong(long value) throws java.io.IOException;
	  public abstract void writeLong(long value);

	  /// <summary>
	  /// Writes a double value to the stream.  The double will be written
	  /// with the following syntax:
	  /// 
	  /// <code><pre>
	  /// D b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the double value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeDouble(double value) throws java.io.IOException;
	  public abstract void writeDouble(double value);

	  /// <summary>
	  /// Writes a date to the stream.
	  /// 
	  /// <code><pre>
	  /// T  b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="time"> the date in milliseconds from the epoch in UTC </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeUTCDate(long time) throws java.io.IOException;
	  public abstract void writeUTCDate(long time);

	  /// <summary>
	  /// Writes a null value to the stream.
	  /// The null will be written with the following syntax
	  /// 
	  /// <code><pre>
	  /// N
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeNull() throws java.io.IOException;
	  public abstract void writeNull();

	  /// <summary>
	  /// Writes a string value to the stream using UTF-8 encoding.
	  /// The string will be written with the following syntax:
	  /// 
	  /// <code><pre>
	  /// S b16 b8 string-value
	  /// </pre></code>
	  /// 
	  /// If the value is null, it will be written as
	  /// 
	  /// <code><pre>
	  /// N
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeString(String value) throws java.io.IOException;
	  public abstract void WriteString(string value);

	  /// <summary>
	  /// Writes a string value to the stream using UTF-8 encoding.
	  /// The string will be written with the following syntax:
	  /// 
	  /// <code><pre>
	  /// S b16 b8 string-value
	  /// </pre></code>
	  /// 
	  /// If the value is null, it will be written as
	  /// 
	  /// <code><pre>
	  /// N
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeString(char [] buffer, int offset, int length) throws java.io.IOException;
	  public abstract void WriteString(char[] buffer, int offset, int length);

	  /// <summary>
	  /// Writes a byte array to the stream.
	  /// The array will be written with the following syntax:
	  /// 
	  /// <code><pre>
	  /// B b16 b18 bytes
	  /// </pre></code>
	  /// 
	  /// If the value is null, it will be written as
	  /// 
	  /// <code><pre>
	  /// N
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeBytes(byte [] buffer) throws java.io.IOException;
	  public abstract void writeBytes(sbyte[] buffer);
	  /// <summary>
	  /// Writes a byte array to the stream.
	  /// The array will be written with the following syntax:
	  /// 
	  /// <code><pre>
	  /// B b16 b18 bytes
	  /// </pre></code>
	  /// 
	  /// If the value is null, it will be written as
	  /// 
	  /// <code><pre>
	  /// N
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeBytes(byte [] buffer, int offset, int length) throws java.io.IOException;
	  public abstract void writeBytes(sbyte[] buffer, int offset, int length);

	  /// <summary>
	  /// Writes a byte buffer to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeByteBufferStart() throws java.io.IOException;
	  public abstract void writeByteBufferStart();

	  /// <summary>
	  /// Writes a byte buffer to the stream.
	  /// 
	  /// <code><pre>
	  /// b b16 b18 bytes
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeByteBufferPart(byte [] buffer, int offset, int length) throws java.io.IOException;
	  public abstract void writeByteBufferPart(sbyte[] buffer, int offset, int length);

	  /// <summary>
	  /// Writes the last chunk of a byte buffer to the stream.
	  /// 
	  /// <code><pre>
	  /// b b16 b18 bytes
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the string value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeByteBufferEnd(byte [] buffer, int offset, int length) throws java.io.IOException;
	  public abstract void writeByteBufferEnd(sbyte[] buffer, int offset, int length);

	  /// <summary>
	  /// Writes a reference.
	  /// 
	  /// <code><pre>
	  /// Q int
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the integer value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract void writeRef(int value) throws java.io.IOException;
	  protected internal abstract void writeRef(int value);

	  /// <summary>
	  /// Removes a reference.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract boolean removeRef(Object obj) throws java.io.IOException;
	  public abstract bool removeRef(object obj);

	  /// <summary>
	  /// Replaces a reference from one object to another.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract boolean replaceRef(Object oldRef, Object newRef) throws java.io.IOException;
	  public abstract bool replaceRef(object oldRef, object newRef);

	  /// <summary>
	  /// Adds an object to the reference list.  If the object already exists,
	  /// writes the reference, otherwise, the caller is responsible for
	  /// the serialization.
	  /// 
	  /// <code><pre>
	  /// R b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="object"> the object to add as a reference.
	  /// </param>
	  /// <returns> true if the object has already been written. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract boolean addRef(Object object) throws java.io.IOException;
	  public abstract bool addRef(object @object);

	  /// <summary>
	  /// Resets the references for streaming.
	  /// </summary>
	  public virtual void resetReferences()
	  {
	  }

	  /// <summary>
	  /// Writes a generic object to the output stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeObject(Object object) throws java.io.IOException;
	  public abstract void writeObject(object @object);

	  /// <summary>
	  /// Writes the list header to the stream.  List writers will call
	  /// <code>writeListBegin</code> followed by the list contents and then
	  /// call <code>writeListEnd</code>.
	  /// 
	  /// <code><pre>
	  /// V
	  ///   x13 java.util.ArrayList   # type
	  ///   x93                       # length=3
	  ///   x91                       # 1
	  ///   x92                       # 2
	  ///   x93                       # 3
	  /// &lt;/list>
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract boolean writeListBegin(int length, String type) throws java.io.IOException;
	  public abstract bool writeListBegin(int length, string type);

	  /// <summary>
	  /// Writes the tail of the list to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeListEnd() throws java.io.IOException;
	  public abstract void writeListEnd();

	  /// <summary>
	  /// Writes the map header to the stream.  Map writers will call
	  /// <code>writeMapBegin</code> followed by the map contents and then
	  /// call <code>writeMapEnd</code>.
	  /// 
	  /// <code><pre>
	  /// M type (<key> <value>)* Z
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeMapBegin(String type) throws java.io.IOException;
	  public abstract void writeMapBegin(string type);

	  /// <summary>
	  /// Writes the tail of the map to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeMapEnd() throws java.io.IOException;
	  public abstract void writeMapEnd();

	  /// <summary>
	  /// Writes the object header to the stream (for Hessian 2.0), or a
	  /// Map for Hessian 1.0.  Object writers will call
	  /// <code>writeObjectBegin</code> followed by the map contents and then
	  /// call <code>writeObjectEnd</code>.
	  /// 
	  /// <code><pre>
	  /// C type int <key>*
	  /// C int <value>*
	  /// </pre></code>
	  /// </summary>
	  /// <returns> true if the object has already been defined. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int writeObjectBegin(String type) throws java.io.IOException
	  public virtual int writeObjectBegin(string type)
	  {
		writeMapBegin(type);

		return -2;
	  }

	  /// <summary>
	  /// Writes the end of the class.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeClassFieldLength(int len) throws java.io.IOException
	  public virtual void writeClassFieldLength(int len)
	  {
	  }

	  /// <summary>
	  /// Writes the tail of the object to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObjectEnd() throws java.io.IOException
	  public virtual void writeObjectEnd()
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeReply(Object o) throws java.io.IOException
	  public virtual void writeReply(object o)
	  {
		startReply();
		writeObject(o);
		completeReply();
	  }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startReply() throws java.io.IOException
	  public virtual void startReply()
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeReply() throws java.io.IOException
	  public virtual void completeReply()
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeFault(String code, String message, Object detail) throws java.io.IOException
	  public virtual void writeFault(string code, string message, object detail)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void flush() throws java.io.IOException
	  public virtual void flush()
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public virtual void close()
	  {
	  }
	}

}