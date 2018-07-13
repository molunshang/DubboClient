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
	/// Output stream for Hessian requests, compatible with microedition
	/// Java.  It only uses classes and types available in JDK.
	/// 
	/// <para>Since HessianOutput does not depend on any classes other than
	/// in the JDK, it can be extracted independently into a smaller package.
	/// 
	/// </para>
	/// <para>HessianOutput is unbuffered, so any client needs to provide
	/// its own buffering.
	/// 
	/// <pre>
	/// OutputStream os = ...; // from http connection
	/// HessianOutput out = new HessianOutput(os);
	/// String value;
	/// 
	/// out.startCall("hello");  // start hello call
	/// out.writeString("arg1"); // write a string argument
	/// out.completeCall();      // complete the call
	/// </pre>
	/// </para>
	/// </summary>
	public class HessianOutput : AbstractHessianOutput
	{
	  // the output stream/
	  protected internal System.IO.Stream os;
	  // map of references
	  private IdentityHashMap _refs;
	  private int _version = 1;

	  /// <summary>
	  /// Creates a new Hessian output stream, initialized with an
	  /// underlying output stream.
	  /// </summary>
	  /// <param name="os"> the underlying output stream. </param>
	  public HessianOutput(System.IO.Stream os)
	  {
		init(os);
	  }

	  /// <summary>
	  /// Creates an uninitialized Hessian output stream.
	  /// </summary>
	  public HessianOutput()
	  {
	  }

	  /// <summary>
	  /// Initializes the output
	  /// </summary>
	  public override void init(System.IO.Stream os)
	  {
		this.os = os;

		_refs = null;

		if (_serializerFactory == null)
		{
		  _serializerFactory = new SerializerFactory();
		}
	  }

	  /// <summary>
	  /// Sets the client's version.
	  /// </summary>
	  public virtual int Version
	  {
		  set
		  {
			_version = value;
		  }
	  }

	  /// <summary>
	  /// Writes a complete method call.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(String method, Object [] args) throws java.io.IOException
	  public override void call(string method, object[] args)
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
	  /// Starts the method call.  Clients would use <code>startCall</code>
	  /// instead of <code>call</code> if they wanted finer control over
	  /// writing the arguments, or needed to write headers.
	  /// 
	  /// <code><pre>
	  /// c major minor
	  /// m b16 b8 method-name
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startCall(String method, int length) throws java.io.IOException
	  public override void startCall(string method, int length)
	  {
		os.WriteByte('c');
		os.WriteByte(_version);
		os.WriteByte(0);

		os.WriteByte('m');
		int len = method.Length;
		os.WriteByte(len >> 8);
		os.WriteByte(len);
		printString(method, 0, len);
	  }

	  /// <summary>
	  /// Writes the call tag.  This would be followed by the
	  /// headers and the method tag.
	  /// 
	  /// <code><pre>
	  /// c major minor
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startCall() throws java.io.IOException
	  public override void startCall()
	  {
		os.WriteByte('c');
		os.WriteByte(0);
		os.WriteByte(1);
	  }

	  /// <summary>
	  /// Writes the method tag.
	  /// 
	  /// <code><pre>
	  /// m b16 b8 method-name
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMethod(String method) throws java.io.IOException
	  public override void writeMethod(string method)
	  {
		os.WriteByte('m');
		int len = method.Length;
		os.WriteByte(len >> 8);
		os.WriteByte(len);
		printString(method, 0, len);
	  }

	  /// <summary>
	  /// Completes.
	  /// 
	  /// <code><pre>
	  /// z
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeCall() throws java.io.IOException
	  public override void completeCall()
	  {
		os.WriteByte('z');
	  }

	  /// <summary>
	  /// Starts the reply
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// r
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startReply() throws java.io.IOException
	  public override void startReply()
	  {
		os.WriteByte('r');
		os.WriteByte(1);
		os.WriteByte(0);
	  }

	  /// <summary>
	  /// Completes reading the reply
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// z
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeReply() throws java.io.IOException
	  public override void completeReply()
	  {
		os.WriteByte('z');
	  }

	  /// <summary>
	  /// Writes a header name.  The header value must immediately follow.
	  /// 
	  /// <code><pre>
	  /// H b16 b8 foo <em>value</em>
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeHeader(String name) throws java.io.IOException
	  public override void writeHeader(string name)
	  {
		int len = name.Length;

		os.WriteByte('H');
		os.WriteByte(len >> 8);
		os.WriteByte(len);

		printString(name);
	  }

	  /// <summary>
	  /// Writes a fault.  The fault will be written
	  /// as a descriptive string followed by an object:
	  /// 
	  /// <code><pre>
	  /// f
	  /// &lt;string>code
	  /// &lt;string>the fault code
	  /// 
	  /// &lt;string>message
	  /// &lt;string>the fault mesage
	  /// 
	  /// &lt;string>detail
	  /// mt\x00\xnnjavax.ejb.FinderException
	  ///     ...
	  /// z
	  /// z
	  /// </pre></code>
	  /// </summary>
	  /// <param name="code"> the fault code, a three digit </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeFault(String code, String message, Object detail) throws java.io.IOException
	  public override void writeFault(string code, string message, object detail)
	  {
		os.WriteByte('f');
		WriteString("code");
		WriteString(code);

		WriteString("message");
		WriteString(message);

		if (detail != null)
		{
		  WriteString("detail");
		  writeObject(detail);
		}
		os.WriteByte('z');
	  }

	  /// <summary>
	  /// Writes any object to the output stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObject(Object object) throws java.io.IOException
	  public override void writeObject(object @object)
	  {
		if (@object == null)
		{
		  writeNull();
		  return;
		}

		Serializer serializer;

		serializer = _serializerFactory.getSerializer(@object.GetType());

		serializer.writeObject(@object, this);
	  }

	  /// <summary>
	  /// Writes the list header to the stream.  List writers will call
	  /// <code>writeListBegin</code> followed by the list contents and then
	  /// call <code>writeListEnd</code>.
	  /// 
	  /// <code><pre>
	  /// V
	  /// t b16 b8 type
	  /// l b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean writeListBegin(int length, String type) throws java.io.IOException
	  public override bool writeListBegin(int length, string type)
	  {
		os.WriteByte('V');

		if (!string.ReferenceEquals(type, null))
		{
		  os.WriteByte('t');
		  printLenString(type);
		}

		if (length >= 0)
		{
		  os.WriteByte('l');
		  os.WriteByte(length >> 24);
		  os.WriteByte(length >> 16);
		  os.WriteByte(length >> 8);
		  os.WriteByte(length);
		}

		return true;
	  }

	  /// <summary>
	  /// Writes the tail of the list to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeListEnd() throws java.io.IOException
	  public override void writeListEnd()
	  {
		os.WriteByte('z');
	  }

	  /// <summary>
	  /// Writes the map header to the stream.  Map writers will call
	  /// <code>writeMapBegin</code> followed by the map contents and then
	  /// call <code>writeMapEnd</code>.
	  /// 
	  /// <code><pre>
	  /// Mt b16 b8 (<key> <value>)z
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMapBegin(String type) throws java.io.IOException
	  public override void writeMapBegin(string type)
	  {
		os.WriteByte('M');
		os.WriteByte('t');
		printLenString(type);
	  }

	  /// <summary>
	  /// Writes the tail of the map to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMapEnd() throws java.io.IOException
	  public override void writeMapEnd()
	  {
		os.WriteByte('z');
	  }

	  /// <summary>
	  /// Writes a remote object reference to the stream.  The type is the
	  /// type of the remote interface.
	  /// 
	  /// <code><pre>
	  /// 'r' 't' b16 b8 type url
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeRemote(String type, String url) throws java.io.IOException
	  public virtual void writeRemote(string type, string url)
	  {
		os.WriteByte('r');
		os.WriteByte('t');
		printLenString(type);
		os.WriteByte('S');
		printLenString(url);
	  }

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
//ORIGINAL LINE: public void writeBoolean(boolean value) throws java.io.IOException
	  public override void writeBoolean(bool value)
	  {
		if (value)
		{
		  os.WriteByte('T');
		}
		else
		{
		  os.WriteByte('F');
		}
	  }

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
//ORIGINAL LINE: public void writeInt(int value) throws java.io.IOException
	  public override void writeInt(int value)
	  {
		os.WriteByte('I');
		os.WriteByte(value >> 24);
		os.WriteByte(value >> 16);
		os.WriteByte(value >> 8);
		os.WriteByte(value);
	  }

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
//ORIGINAL LINE: public void writeLong(long value) throws java.io.IOException
	  public override void writeLong(long value)
	  {
		os.WriteByte('L');
		os.WriteByte((sbyte)(value >> 56));
		os.WriteByte((sbyte)(value >> 48));
		os.WriteByte((sbyte)(value >> 40));
		os.WriteByte((sbyte)(value >> 32));
		os.WriteByte((sbyte)(value >> 24));
		os.WriteByte((sbyte)(value >> 16));
		os.WriteByte((sbyte)(value >> 8));
		os.WriteByte((sbyte)(value));
	  }

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
//ORIGINAL LINE: public void writeDouble(double value) throws java.io.IOException
	  public override void writeDouble(double value)
	  {
		long bits = Double.doubleToLongBits(value);

		os.WriteByte('D');
		os.WriteByte((sbyte)(bits >> 56));
		os.WriteByte((sbyte)(bits >> 48));
		os.WriteByte((sbyte)(bits >> 40));
		os.WriteByte((sbyte)(bits >> 32));
		os.WriteByte((sbyte)(bits >> 24));
		os.WriteByte((sbyte)(bits >> 16));
		os.WriteByte((sbyte)(bits >> 8));
		os.WriteByte((sbyte)(bits));
	  }

	  /// <summary>
	  /// Writes a date to the stream.
	  /// 
	  /// <code><pre>
	  /// T  b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="time"> the date in milliseconds from the epoch in UTC </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeUTCDate(long time) throws java.io.IOException
	  public override void writeUTCDate(long time)
	  {
		os.WriteByte('d');
		os.WriteByte((sbyte)(time >> 56));
		os.WriteByte((sbyte)(time >> 48));
		os.WriteByte((sbyte)(time >> 40));
		os.WriteByte((sbyte)(time >> 32));
		os.WriteByte((sbyte)(time >> 24));
		os.WriteByte((sbyte)(time >> 16));
		os.WriteByte((sbyte)(time >> 8));
		os.WriteByte((sbyte)(time));
	  }

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
//ORIGINAL LINE: public void writeNull() throws java.io.IOException
	  public override void writeNull()
	  {
		os.WriteByte('N');
	  }

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
//ORIGINAL LINE: public void writeString(String value) throws java.io.IOException
	  public override void WriteString(string value)
	  {
		if (string.ReferenceEquals(value, null))
		{
		  os.WriteByte('N');
		}
		else
		{
		  int length = value.Length;
		  int offset = 0;

		  while (length > 0x8000)
		  {
			int sublen = 0x8000;

		// chunk can't end in high surrogate
		char tail = value[offset + sublen - 1];

		if (0xd800 <= tail && tail <= 0xdbff)
		{
		  sublen--;
		}

			os.WriteByte('s');
			os.WriteByte(sublen >> 8);
			os.WriteByte(sublen);

			printString(value, offset, sublen);

			length -= sublen;
			offset += sublen;
		  }

		  os.WriteByte('S');
		  os.WriteByte(length >> 8);
		  os.WriteByte(length);

		  printString(value, offset, length);
		}
	  }

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
//ORIGINAL LINE: public void writeString(char [] buffer, int offset, int length) throws java.io.IOException
	  public override void WriteString(char[] buffer, int offset, int length)
	  {
		if (buffer == null)
		{
		  os.WriteByte('N');
		}
		else
		{
		  while (length > 0x8000)
		  {
			int sublen = 0x8000;

		// chunk can't end in high surrogate
		char tail = buffer[offset + sublen - 1];

		if (0xd800 <= tail && tail <= 0xdbff)
		{
		  sublen--;
		}

			os.WriteByte('s');
			os.WriteByte(sublen >> 8);
			os.WriteByte(sublen);

			printString(buffer, offset, sublen);

			length -= sublen;
			offset += sublen;
		  }

		  os.WriteByte('S');
		  os.WriteByte(length >> 8);
		  os.WriteByte(length);

		  printString(buffer, offset, length);
		}
	  }

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
//ORIGINAL LINE: public void writeBytes(byte [] buffer) throws java.io.IOException
	  public override void writeBytes(sbyte[] buffer)
	  {
		if (buffer == null)
		{
		  os.WriteByte('N');
		}
		else
		{
		  writeBytes(buffer, 0, buffer.Length);
		}
	  }

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
//ORIGINAL LINE: public void writeBytes(byte [] buffer, int offset, int length) throws java.io.IOException
	  public override void writeBytes(sbyte[] buffer, int offset, int length)
	  {
		if (buffer == null)
		{
		  os.WriteByte('N');
		}
		else
		{
		  while (length > 0x8000)
		  {
			int sublen = 0x8000;

			os.WriteByte('b');
			os.WriteByte(sublen >> 8);
			os.WriteByte(sublen);

			os.Write(buffer, offset, sublen);

			length -= sublen;
			offset += sublen;
		  }

		  os.WriteByte('B');
		  os.WriteByte(length >> 8);
		  os.WriteByte(length);
		  os.Write(buffer, offset, length);
		}
	  }

	  /// <summary>
	  /// Writes a byte buffer to the stream.
	  /// 
	  /// <code><pre>
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeByteBufferStart() throws java.io.IOException
	  public override void writeByteBufferStart()
	  {
	  }

	  /// <summary>
	  /// Writes a byte buffer to the stream.
	  /// 
	  /// <code><pre>
	  /// b b16 b18 bytes
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeByteBufferPart(byte [] buffer, int offset, int length) throws java.io.IOException
	  public override void writeByteBufferPart(sbyte[] buffer, int offset, int length)
	  {
		while (length > 0)
		{
		  int sublen = length;

		  if (0x8000 < sublen)
		  {
		sublen = 0x8000;
		  }

		  os.WriteByte('b');
		  os.WriteByte(sublen >> 8);
		  os.WriteByte(sublen);

		  os.Write(buffer, offset, sublen);

		  length -= sublen;
		  offset += sublen;
		}
	  }

	  /// <summary>
	  /// Writes a byte buffer to the stream.
	  /// 
	  /// <code><pre>
	  /// b b16 b18 bytes
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeByteBufferEnd(byte [] buffer, int offset, int length) throws java.io.IOException
	  public override void writeByteBufferEnd(sbyte[] buffer, int offset, int length)
	  {
		writeBytes(buffer, offset, length);
	  }

	  /// <summary>
	  /// Writes a reference.
	  /// 
	  /// <code><pre>
	  /// R b32 b24 b16 b8
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the integer value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeRef(int value) throws java.io.IOException
	  public override void writeRef(int value)
	  {
		os.WriteByte('R');
		os.WriteByte(value >> 24);
		os.WriteByte(value >> 16);
		os.WriteByte(value >> 8);
		os.WriteByte(value);
	  }

	  /// <summary>
	  /// Writes a placeholder.
	  /// 
	  /// <code><pre>
	  /// P
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writePlaceholder() throws java.io.IOException
	  public virtual void writePlaceholder()
	  {
		os.WriteByte('P');
	  }

	  /// <summary>
	  /// If the object has already been written, just write its ref.
	  /// </summary>
	  /// <returns> true if we're writing a ref. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean addRef(Object object) throws java.io.IOException
	  public override bool addRef(object @object)
	  {
		if (_refs == null)
		{
		  _refs = new IdentityHashMap();
		}

		int? @ref = (int?) _refs.get(@object);

		if (@ref != null)
		{
		  int value = @ref.Value;

		  writeRef(value);
		  return true;
		}
		else
		{
		  _refs.put(@object, new int?(_refs.size()));

		  return false;
		}
	  }

	  /// <summary>
	  /// Resets the references for streaming.
	  /// </summary>
	  public override void resetReferences()
	  {
		if (_refs != null)
		{
		  _refs.clear();
		}
	  }

	  /// <summary>
	  /// Removes a reference.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean removeRef(Object obj) throws java.io.IOException
	  public override bool removeRef(object obj)
	  {
		if (_refs != null)
		{
		  _refs.remove(obj);

		  return true;
		}
		else
		{
		  return false;
		}
	  }

	  /// <summary>
	  /// Replaces a reference from one object to another.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean replaceRef(Object oldRef, Object newRef) throws java.io.IOException
	  public override bool replaceRef(object oldRef, object newRef)
	  {
		int? value = (int?) _refs.remove(oldRef);

		if (value != null)
		{
		  _refs.put(newRef, value);
		  return true;
		}
		else
		{
		  return false;
		}
	  }

	  /// <summary>
	  /// Prints a string to the stream, encoded as UTF-8 with preceeding length
	  /// </summary>
	  /// <param name="v"> the string to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void printLenString(String v) throws java.io.IOException
	  public virtual void printLenString(string v)
	  {
		if (string.ReferenceEquals(v, null))
		{
		  os.WriteByte(0);
		  os.WriteByte(0);
		}
		else
		{
		  int len = v.Length;
		  os.WriteByte(len >> 8);
		  os.WriteByte(len);

		  printString(v, 0, len);
		}
	  }

	  /// <summary>
	  /// Prints a string to the stream, encoded as UTF-8
	  /// </summary>
	  /// <param name="v"> the string to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void printString(String v) throws java.io.IOException
	  public virtual void printString(string v)
	  {
		printString(v, 0, v.Length);
	  }

	  /// <summary>
	  /// Prints a string to the stream, encoded as UTF-8
	  /// </summary>
	  /// <param name="v"> the string to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void printString(String v, int offset, int length) throws java.io.IOException
	  public virtual void printString(string v, int offset, int length)
	  {
		for (int i = 0; i < length; i++)
		{
		  char ch = v[i + offset];

		  if (ch < 0x80)
		  {
			os.WriteByte(ch);
		  }
		  else if (ch < 0x800)
		  {
			os.WriteByte(0xc0 + ((ch >> 6) & 0x1f));
			os.WriteByte(0x80 + (ch & 0x3f));
		  }
		  else
		  {
			os.WriteByte(0xe0 + ((ch >> 12) & 0xf));
			os.WriteByte(0x80 + ((ch >> 6) & 0x3f));
			os.WriteByte(0x80 + (ch & 0x3f));
		  }
		}
	  }

	  /// <summary>
	  /// Prints a string to the stream, encoded as UTF-8
	  /// </summary>
	  /// <param name="v"> the string to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void printString(char [] v, int offset, int length) throws java.io.IOException
	  public virtual void printString(char[] v, int offset, int length)
	  {
		for (int i = 0; i < length; i++)
		{
		  char ch = v[i + offset];

		  if (ch < 0x80)
		  {
			os.WriteByte(ch);
		  }
		  else if (ch < 0x800)
		  {
			os.WriteByte(0xc0 + ((ch >> 6) & 0x1f));
			os.WriteByte(0x80 + (ch & 0x3f));
		  }
		  else
		  {
			os.WriteByte(0xe0 + ((ch >> 12) & 0xf));
			os.WriteByte(0x80 + ((ch >> 6) & 0x3f));
			os.WriteByte(0x80 + (ch & 0x3f));
		  }
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void flush() throws java.io.IOException
	  public override void flush()
	  {
		if (this.os != null)
		{
		  this.os.Flush();
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public override void close()
	  {
		if (this.os != null)
		{
		  this.os.Flush();
		}
	  }
	}

}