using System;
using System.Collections;

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

	using IdentityIntMap = com.alibaba.com.caucho.hessian.util.IdentityIntMap;


	/// <summary>
	/// Output stream for Hessian 2 requests.
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
	/// Hessian2Output out = new Hessian2Output(os);
	/// String value;
	/// 
	/// out.startCall("hello", 1); // start hello call
	/// out.writeString("arg1");   // write a string argument
	/// out.completeCall();        // complete the call
	/// </pre>
	/// </para>
	/// </summary>
	public class Hessian2Output : AbstractHessianOutput, Hessian2Constants
	{
	  // the output stream/
	  protected internal System.IO.Stream _os;

	  // map of references
	  private IdentityIntMap _refs = new IdentityIntMap();

	  private bool _isCloseStreamOnClose;

	  // map of classes
	  private Hashtable _classRefs;

	  // map of types
	  private Hashtable _typeRefs;

	  public const int SIZE = 4096;

	  private readonly sbyte[] _buffer = new sbyte[SIZE];
	  private int _offset;

	  private bool _isStreaming;

	  /// <summary>
	  /// Creates a new Hessian output stream, initialized with an
	  /// underlying output stream.
	  /// </summary>
	  /// <param name="os"> the underlying output stream. </param>
	  public Hessian2Output(System.IO.Stream os)
	  {
		_os = os;
	  }

	  public virtual bool CloseStreamOnClose
	  {
		  set
		  {
			_isCloseStreamOnClose = value;
		  }
		  get
		  {
			return _isCloseStreamOnClose;
		  }
	  }



	  /// <summary>
	  /// Writes a complete method call.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void call(String method, Object [] args) throws java.io.IOException
	  public override void call(string method, object[] args)
	  {
		int length = args != null ? args.Length : 0;

		startCall(method, length);

		for (int i = 0; i < args.Length; i++)
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
	  /// C
	  /// string # method name
	  /// int    # arg count
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startCall(String method, int length) throws java.io.IOException
	  public override void startCall(string method, int length)
	  {
		int offset = _offset;

		if (SIZE < offset + 32)
		{
		  flush();
		  offset = _offset;
		}

		sbyte[] buffer = _buffer;

		buffer[_offset++] = (sbyte) 'C';

		WriteString(method);
		writeInt(length);
	  }

	  /// <summary>
	  /// Writes the call tag.  This would be followed by the
	  /// method and the arguments
	  /// 
	  /// <code><pre>
	  /// C
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startCall() throws java.io.IOException
	  public override void startCall()
	  {
		flushIfFull();

		_buffer[_offset++] = (sbyte) 'C';
	  }

	  /// <summary>
	  /// Starts an envelope.
	  /// 
	  /// <code><pre>
	  /// E major minor
	  /// m b16 b8 method-name
	  /// </pre></code>
	  /// </summary>
	  /// <param name="method"> the method name to call. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startEnvelope(String method) throws java.io.IOException
	  public virtual void startEnvelope(string method)
	  {
		int offset = _offset;

		if (SIZE < offset + 32)
		{
		  flush();
		  offset = _offset;
		}

		_buffer[_offset++] = (sbyte) 'E';

		WriteString(method);
	  }

	  /// <summary>
	  /// Completes an envelope.
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// Z
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeEnvelope() throws java.io.IOException
	  public virtual void completeEnvelope()
	  {
		flushIfFull();

		_buffer[_offset++] = (sbyte) 'Z';
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
//ORIGINAL LINE: public void writeMethod(String method) throws java.io.IOException
	  public override void writeMethod(string method)
	  {
		WriteString(method);
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
		/*
		flushIfFull();
		
		_buffer[_offset++] = (byte) 'Z';
		*/
	  }

	  /// <summary>
	  /// Starts the reply
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// R
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startReply() throws java.io.IOException
	  public override void startReply()
	  {
		writeVersion();

		flushIfFull();

		_buffer[_offset++] = (sbyte) 'R';
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeVersion() throws java.io.IOException
	  public virtual void writeVersion()
	  {
		flushIfFull();
		_buffer[_offset++] = (sbyte) 'H';
		_buffer[_offset++] = (sbyte) 2;
		_buffer[_offset++] = (sbyte) 0;
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
	  }

	  /// <summary>
	  /// Starts a packet
	  /// 
	  /// <para>A message contains several objects encapsulated by a length</para>
	  /// 
	  /// <pre>
	  /// p x02 x00
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startMessage() throws java.io.IOException
	  public virtual void startMessage()
	  {
		flushIfFull();

		_buffer[_offset++] = (sbyte) 'p';
		_buffer[_offset++] = (sbyte) 2;
		_buffer[_offset++] = (sbyte) 0;
	  }

	  /// <summary>
	  /// Completes reading the message
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// z
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeMessage() throws java.io.IOException
	  public virtual void completeMessage()
	  {
		flushIfFull();

		_buffer[_offset++] = (sbyte) 'z';
	  }

	  /// <summary>
	  /// Writes a fault.  The fault will be written
	  /// as a descriptive string followed by an object:
	  /// 
	  /// <code><pre>
	  /// F map
	  /// </pre></code>
	  /// 
	  /// <code><pre>
	  /// F H
	  /// \x04code
	  /// \x10the fault code
	  /// 
	  /// \x07message
	  /// \x11the fault message
	  /// 
	  /// \x06detail
	  /// M\xnnjavax.ejb.FinderException
	  ///     ...
	  /// Z
	  /// Z
	  /// </pre></code>
	  /// </summary>
	  /// <param name="code"> the fault code, a three digit </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeFault(String code, String message, Object detail) throws java.io.IOException
	  public override void writeFault(string code, string message, object detail)
	  {
		flushIfFull();

		writeVersion();

		_buffer[_offset++] = (sbyte) 'F';
		_buffer[_offset++] = (sbyte) 'H';

		_refs.put(new Hashtable(), _refs.size());

		WriteString("code");
		WriteString(code);

		WriteString("message");
		WriteString(message);

		if (detail != null)
		{
		  WriteString("detail");
		  writeObject(detail);
		}

		flushIfFull();
		_buffer[_offset++] = (sbyte) 'Z';
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

		serializer = findSerializerFactory().getSerializer(@object.GetType());

		serializer.writeObject(@object, this);
	  }

	  /// <summary>
	  /// Writes the list header to the stream.  List writers will call
	  /// <code>writeListBegin</code> followed by the list contents and then
	  /// call <code>writeListEnd</code>.
	  /// 
	  /// <code><pre>
	  /// list ::= V type value* Z
	  ///      ::= v type int value*
	  /// </pre></code>
	  /// </summary>
	  /// <returns> true for variable lists, false for fixed lists </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean writeListBegin(int length, String type) throws java.io.IOException
	  public override bool writeListBegin(int length, string type)
	  {
		flushIfFull();

		if (length < 0)
		{
		  if (!string.ReferenceEquals(type, null))
		  {
		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_LIST_VARIABLE;
		writeType(type);
		  }
		  else
		  {
		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_LIST_VARIABLE_UNTYPED;
		  }

		  return true;
		}
		else if (length <= Hessian2Constants_Fields.LIST_DIRECT_MAX)
		{
		  if (!string.ReferenceEquals(type, null))
		  {
		_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_LIST_DIRECT + length);
		writeType(type);
		  }
		  else
		  {
		_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_LIST_DIRECT_UNTYPED + length);
		  }

		  return false;
		}
		else
		{
		  if (!string.ReferenceEquals(type, null))
		  {
		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_LIST_FIXED;
		writeType(type);
		  }
		  else
		  {
		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_LIST_FIXED_UNTYPED;
		  }

		  writeInt(length);

		  return false;
		}
	  }

	  /// <summary>
	  /// Writes the tail of the list to the stream for a variable-length list.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeListEnd() throws java.io.IOException
	  public override void writeListEnd()
	  {
		flushIfFull();

		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_END;
	  }

	  /// <summary>
	  /// Writes the map header to the stream.  Map writers will call
	  /// <code>writeMapBegin</code> followed by the map contents and then
	  /// call <code>writeMapEnd</code>.
	  /// 
	  /// <code><pre>
	  /// map ::= M type (<value> <value>)* Z
	  ///     ::= H (<value> <value>)* Z
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMapBegin(String type) throws java.io.IOException
	  public override void writeMapBegin(string type)
	  {
		if (SIZE < _offset + 32)
		{
		  flush();
		}

		if (!string.ReferenceEquals(type, null))
		{
		  _buffer[_offset++] = Hessian2Constants_Fields.BC_MAP;

		  writeType(type);
		}
		else
		{
		  _buffer[_offset++] = Hessian2Constants_Fields.BC_MAP_UNTYPED;
		}
	  }

	  /// <summary>
	  /// Writes the tail of the map to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMapEnd() throws java.io.IOException
	  public override void writeMapEnd()
	  {
		if (SIZE < _offset + 32)
		{
		  flush();
		}

		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_END;
	  }

	  /// <summary>
	  /// Writes the object definition
	  /// 
	  /// <code><pre>
	  /// C &lt;string> &lt;int> &lt;string>*
	  /// </pre></code>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int writeObjectBegin(String type) throws java.io.IOException
	  public override int writeObjectBegin(string type)
	  {
		if (_classRefs == null)
		{
		  _classRefs = new Hashtable();
		}

		int? refV = (int?) _classRefs[type];

		if (refV != null)
		{
		  int @ref = refV.Value;

		  if (SIZE < _offset + 32)
		  {
		flush();
		  }

		  if (@ref <= Hessian2Constants_Fields.OBJECT_DIRECT_MAX)
		  {
		_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_OBJECT_DIRECT + @ref);
		  }
		  else
		  {
		_buffer[_offset++] = (sbyte) 'O';
		writeInt(@ref);
		  }

		  return @ref;
		}
		else
		{
		  int @ref = _classRefs.Count;

		  _classRefs[type] = Convert.ToInt32(@ref);

		  if (SIZE < _offset + 32)
		  {
		flush();
		  }

		  _buffer[_offset++] = (sbyte) 'C';

		  WriteString(type);

		  return -1;
		}
	  }

	  /// <summary>
	  /// Writes the tail of the class definition to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeClassFieldLength(int len) throws java.io.IOException
	  public override void writeClassFieldLength(int len)
	  {
		writeInt(len);
	  }

	  /// <summary>
	  /// Writes the tail of the object definition to the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObjectEnd() throws java.io.IOException
	  public override void writeObjectEnd()
	  {
	  }

	  /// <summary>
	  /// <code><pre>
	  /// type ::= string
	  ///      ::= int
	  /// </code></pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeType(String type) throws java.io.IOException
	  private void writeType(string type)
	  {
		flushIfFull();

		int len = type.Length;
		if (len == 0)
		{
		  throw new System.ArgumentException("empty type is not allowed");
		}

		if (_typeRefs == null)
		{
		  _typeRefs = new Hashtable();
		}

		int? typeRefV = (int?) _typeRefs[type];

		if (typeRefV != null)
		{
		  int typeRef = typeRefV.Value;

		  writeInt(typeRef);
		}
		else
		{
		  _typeRefs[type] = Convert.ToInt32(_typeRefs.Count);

		  WriteString(type);
		}
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
		if (SIZE < _offset + 16)
		{
		  flush();
		}

		if (value)
		{
		  _buffer[_offset++] = (sbyte) 'T';
		}
		else
		{
		  _buffer[_offset++] = (sbyte) 'F';
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
		int offset = _offset;
		sbyte[] buffer = _buffer;

		if (SIZE <= offset + 16)
		{
		  flush();
		  offset = _offset;
		}

		if (Hessian2Constants_Fields.INT_DIRECT_MIN <= value && value <= Hessian2Constants_Fields.INT_DIRECT_MAX)
		{
		  buffer[offset++] = (sbyte)(value + Hessian2Constants_Fields.BC_INT_ZERO);
		}
		else if (Hessian2Constants_Fields.INT_BYTE_MIN <= value && value <= Hessian2Constants_Fields.INT_BYTE_MAX)
		{
		  buffer[offset++] = (sbyte)(Hessian2Constants_Fields.BC_INT_BYTE_ZERO + (value >> 8));
		  buffer[offset++] = (sbyte)(value);
		}
		else if (Hessian2Constants_Fields.INT_SHORT_MIN <= value && value <= Hessian2Constants_Fields.INT_SHORT_MAX)
		{
		  buffer[offset++] = (sbyte)(Hessian2Constants_Fields.BC_INT_SHORT_ZERO + (value >> 16));
		  buffer[offset++] = (sbyte)(value >> 8);
		  buffer[offset++] = (sbyte)(value);
		}
		else
		{
		  buffer[offset++] = (sbyte)('I');
		  buffer[offset++] = (sbyte)(value >> 24);
		  buffer[offset++] = (sbyte)(value >> 16);
		  buffer[offset++] = (sbyte)(value >> 8);
		  buffer[offset++] = (sbyte)(value);
		}

		_offset = offset;
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
		int offset = _offset;
		sbyte[] buffer = _buffer;

		if (SIZE <= offset + 16)
		{
		  flush();
		  offset = _offset;
		}

		if (Hessian2Constants_Fields.LONG_DIRECT_MIN <= value && value <= Hessian2Constants_Fields.LONG_DIRECT_MAX)
		{
		  buffer[offset++] = (sbyte)(value + Hessian2Constants_Fields.BC_LONG_ZERO);
		}
		else if (Hessian2Constants_Fields.LONG_BYTE_MIN <= value && value <= Hessian2Constants_Fields.LONG_BYTE_MAX)
		{
		  buffer[offset++] = (sbyte)(Hessian2Constants_Fields.BC_LONG_BYTE_ZERO + (value >> 8));
		  buffer[offset++] = (sbyte)(value);
		}
		else if (Hessian2Constants_Fields.LONG_SHORT_MIN <= value && value <= Hessian2Constants_Fields.LONG_SHORT_MAX)
		{
		  buffer[offset++] = (sbyte)(Hessian2Constants_Fields.BC_LONG_SHORT_ZERO + (value >> 16));
		  buffer[offset++] = (sbyte)(value >> 8);
		  buffer[offset++] = (sbyte)(value);
		}
		else if (-0x80000000L <= value && value <= 0x7fffffffL)
		{
		  buffer[offset + 0] = (sbyte) Hessian2Constants_Fields.BC_LONG_INT;
		  buffer[offset + 1] = (sbyte)(value >> 24);
		  buffer[offset + 2] = (sbyte)(value >> 16);
		  buffer[offset + 3] = (sbyte)(value >> 8);
		  buffer[offset + 4] = (sbyte)(value);

		  offset += 5;
		}
		else
		{
		  buffer[offset + 0] = (sbyte) 'L';
		  buffer[offset + 1] = (sbyte)(value >> 56);
		  buffer[offset + 2] = (sbyte)(value >> 48);
		  buffer[offset + 3] = (sbyte)(value >> 40);
		  buffer[offset + 4] = (sbyte)(value >> 32);
		  buffer[offset + 5] = (sbyte)(value >> 24);
		  buffer[offset + 6] = (sbyte)(value >> 16);
		  buffer[offset + 7] = (sbyte)(value >> 8);
		  buffer[offset + 8] = (sbyte)(value);

		  offset += 9;
		}

		_offset = offset;
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
		int offset = _offset;
		sbyte[] buffer = _buffer;

		if (SIZE <= offset + 16)
		{
		  flush();
		  offset = _offset;
		}

		int intValue = (int) value;

		if (intValue == value)
		{
		  if (intValue == 0)
		  {
		buffer[offset++] = (sbyte) Hessian2Constants_Fields.BC_DOUBLE_ZERO;

			_offset = offset;

			return;
		  }
		  else if (intValue == 1)
		  {
		buffer[offset++] = (sbyte) Hessian2Constants_Fields.BC_DOUBLE_ONE;

			_offset = offset;

			return;
		  }
		  else if (-0x80 <= intValue && intValue < 0x80)
		  {
		buffer[offset++] = (sbyte) Hessian2Constants_Fields.BC_DOUBLE_BYTE;
		buffer[offset++] = (sbyte) intValue;

			_offset = offset;

			return;
		  }
		  else if (-0x8000 <= intValue && intValue < 0x8000)
		  {
		buffer[offset + 0] = (sbyte) Hessian2Constants_Fields.BC_DOUBLE_SHORT;
		buffer[offset + 1] = (sbyte)(intValue >> 8);
		buffer[offset + 2] = (sbyte) intValue;

		_offset = offset + 3;

			return;
		  }
		}

		int mills = (int)(value * 1000);

		if (0.001 * mills == value)
		{
		  buffer[offset + 0] = (sbyte)(Hessian2Constants_Fields.BC_DOUBLE_MILL);
		  buffer[offset + 1] = (sbyte)(mills >> 24);
		  buffer[offset + 2] = (sbyte)(mills >> 16);
		  buffer[offset + 3] = (sbyte)(mills >> 8);
		  buffer[offset + 4] = (sbyte)(mills);

		  _offset = offset + 5;

		  return;
		}

		long bits = Double.doubleToLongBits(value);

		buffer[offset + 0] = (sbyte) 'D';
		buffer[offset + 1] = (sbyte)(bits >> 56);
		buffer[offset + 2] = (sbyte)(bits >> 48);
		buffer[offset + 3] = (sbyte)(bits >> 40);
		buffer[offset + 4] = (sbyte)(bits >> 32);
		buffer[offset + 5] = (sbyte)(bits >> 24);
		buffer[offset + 6] = (sbyte)(bits >> 16);
		buffer[offset + 7] = (sbyte)(bits >> 8);
		buffer[offset + 8] = (sbyte)(bits);

		_offset = offset + 9;
	  }

	  /// <summary>
	  /// Writes a date to the stream.
	  /// 
	  /// <code><pre>
	  /// date ::= d   b7 b6 b5 b4 b3 b2 b1 b0
	  ///      ::= x65 b3 b2 b1 b0
	  /// </pre></code>
	  /// </summary>
	  /// <param name="time"> the date in milliseconds from the epoch in UTC </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeUTCDate(long time) throws java.io.IOException
	  public override void writeUTCDate(long time)
	  {
		if (SIZE < _offset + 32)
		{
		  flush();
		}

		int offset = _offset;
		sbyte[] buffer = _buffer;

		if (time % 60000L == 0)
		{
		  // compact date ::= x65 b3 b2 b1 b0

		  long minutes = time / 60000L;

		  if ((minutes >> 31) == 0 || (minutes >> 31) == -1)
		  {
		buffer[offset++] = (sbyte) Hessian2Constants_Fields.BC_DATE_MINUTE;
		buffer[offset++] = ((sbyte)(minutes >> 24));
		buffer[offset++] = ((sbyte)(minutes >> 16));
		buffer[offset++] = ((sbyte)(minutes >> 8));
		buffer[offset++] = ((sbyte)(minutes >> 0));

		_offset = offset;
		return;
		  }
		}

		buffer[offset++] = (sbyte) Hessian2Constants_Fields.BC_DATE;
		buffer[offset++] = ((sbyte)(time >> 56));
		buffer[offset++] = ((sbyte)(time >> 48));
		buffer[offset++] = ((sbyte)(time >> 40));
		buffer[offset++] = ((sbyte)(time >> 32));
		buffer[offset++] = ((sbyte)(time >> 24));
		buffer[offset++] = ((sbyte)(time >> 16));
		buffer[offset++] = ((sbyte)(time >> 8));
		buffer[offset++] = ((sbyte)(time));

		_offset = offset;
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
		int offset = _offset;
		sbyte[] buffer = _buffer;

		if (SIZE <= offset + 16)
		{
		  flush();
		  offset = _offset;
		}

		buffer[offset++] = (sbyte)'N';

		_offset = offset;
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
		int offset = _offset;
		sbyte[] buffer = _buffer;

		if (SIZE <= offset + 16)
		{
		  flush();
		  offset = _offset;
		}

		if (string.ReferenceEquals(value, null))
		{
		  buffer[offset++] = (sbyte) 'N';

		  _offset = offset;
		}
		else
		{
		  int length = value.Length;
		  int strOffset = 0;

		  while (length > 0x8000)
		  {
			int sublen = 0x8000;

		offset = _offset;

		if (SIZE <= offset + 16)
		{
		  flush();
		  offset = _offset;
		}

		// chunk can't end in high surrogate
		char tail = value[strOffset + sublen - 1];

		if (0xd800 <= tail && tail <= 0xdbff)
		{
		  sublen--;
		}

		buffer[offset + 0] = (sbyte) Hessian2Constants_Fields.BC_STRING_CHUNK;
			buffer[offset + 1] = (sbyte)(sublen >> 8);
			buffer[offset + 2] = (sbyte)(sublen);

		_offset = offset + 3;

			printString(value, strOffset, sublen);

			length -= sublen;
			strOffset += sublen;
		  }

		  offset = _offset;

		  if (SIZE <= offset + 16)
		  {
		flush();
		offset = _offset;
		  }

		  if (length <= Hessian2Constants_Fields.STRING_DIRECT_MAX)
		  {
		buffer[offset++] = (sbyte)(Hessian2Constants_Fields.BC_STRING_DIRECT + length);
		  }
		  else if (length <= Hessian2Constants_Fields.STRING_SHORT_MAX)
		  {
		buffer[offset++] = (sbyte)(Hessian2Constants_Fields.BC_STRING_SHORT + (length >> 8));
		buffer[offset++] = (sbyte)(length);
		  }
		  else
		  {
		buffer[offset++] = (sbyte)('S');
		buffer[offset++] = (sbyte)(length >> 8);
		buffer[offset++] = (sbyte)(length);
		  }

		  _offset = offset;

		  printString(value, strOffset, length);
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
		  if (SIZE < _offset + 16)
		  {
		flush();
		  }

		  _buffer[_offset++] = (sbyte)('N');
		}
		else
		{
		  while (length > 0x8000)
		  {
			int sublen = 0x8000;

		if (SIZE < _offset + 16)
		{
		  flush();
		}

		// chunk can't end in high surrogate
		char tail = buffer[offset + sublen - 1];

		if (0xd800 <= tail && tail <= 0xdbff)
		{
		  sublen--;
		}

			_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_STRING_CHUNK;
			_buffer[_offset++] = (sbyte)(sublen >> 8);
			_buffer[_offset++] = (sbyte)(sublen);

			printString(buffer, offset, sublen);

			length -= sublen;
			offset += sublen;
		  }

		  if (SIZE < _offset + 16)
		  {
		flush();
		  }

		  if (length <= Hessian2Constants_Fields.STRING_DIRECT_MAX)
		  {
		_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_STRING_DIRECT + length);
		  }
		  else if (length <= Hessian2Constants_Fields.STRING_SHORT_MAX)
		  {
		_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_STRING_SHORT + (length >> 8));
		_buffer[_offset++] = (sbyte) length;
		  }
		  else
		  {
		_buffer[_offset++] = (sbyte)('S');
		_buffer[_offset++] = (sbyte)(length >> 8);
		_buffer[_offset++] = (sbyte)(length);
		  }

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
		  if (SIZE < _offset + 16)
		  {
		flush();
		  }

		  _buffer[_offset++] = (sbyte)'N';
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
		  if (SIZE < _offset + 16)
		  {
		flushBuffer();
		  }

		  _buffer[_offset++] = (sbyte) 'N';
		}
		else
		{
		  flush();

		  while (SIZE - _offset - 3 < length)
		  {
			int sublen = SIZE - _offset - 3;

			if (sublen < 16)
			{
			  flushBuffer();

			  sublen = SIZE - _offset - 3;

			  if (length < sublen)
			  {
				sublen = length;
			  }
			}

			_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_BINARY_CHUNK;
			_buffer[_offset++] = (sbyte)(sublen >> 8);
			_buffer[_offset++] = (sbyte) sublen;

			Array.Copy(buffer, offset, _buffer, _offset, sublen);
			_offset += sublen;

			length -= sublen;
			offset += sublen;

		flushBuffer();
		  }

		  if (SIZE < _offset + 16)
		  {
			flushBuffer();
		  }

		  if (length <= Hessian2Constants_Fields.BINARY_DIRECT_MAX)
		  {
			_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_BINARY_DIRECT + length);
		  }
		  else if (length <= Hessian2Constants_Fields.BINARY_SHORT_MAX)
		  {
			_buffer[_offset++] = (sbyte)(Hessian2Constants_Fields.BC_BINARY_SHORT + (length >> 8));
			_buffer[_offset++] = (sbyte)(length);
		  }
		  else
		  {
			_buffer[_offset++] = (sbyte) 'B';
			_buffer[_offset++] = (sbyte)(length >> 8);
			_buffer[_offset++] = (sbyte)(length);
		  }

		  Array.Copy(buffer, offset, _buffer, _offset, length);

		  _offset += length;
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

		  flush(); // bypass buffer

		  _os.WriteByte(Hessian2Constants_Fields.BC_BINARY_CHUNK);
		  _os.WriteByte(sublen >> 8);
		  _os.WriteByte(sublen);

		  _os.Write(buffer, offset, sublen);

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
	  /// Returns an output stream to write binary data.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.OutputStream getBytesOutputStream() throws java.io.IOException
	  public virtual System.IO.Stream getBytesOutputStream()
	  {
		return new BytesOutputStream(this);
	  }

	  /// <summary>
	  /// Writes a reference.
	  /// 
	  /// <code><pre>
	  /// x51 &lt;int>
	  /// </pre></code>
	  /// </summary>
	  /// <param name="value"> the integer value to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void writeRef(int value) throws java.io.IOException
	  protected internal override void writeRef(int value)
	  {
		if (SIZE < _offset + 16)
		{
		  flush();
		}

		_buffer[_offset++] = (sbyte) Hessian2Constants_Fields.BC_REF;

		writeInt(value);
	  }

	  /// <summary>
	  /// If the object has already been written, just write its ref.
	  /// </summary>
	  /// <returns> true if we're writing a ref. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean addRef(Object object) throws java.io.IOException
	  public override bool addRef(object @object)
	  {
		int @ref = _refs.get(@object);

		if (@ref >= 0)
		{
		  writeRef(@ref);

		  return true;
		}
		else
		{
		  _refs.put(@object, _refs.size());

		  return false;
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
		  _refs.put(newRef, value.Value);
		  return true;
		}
		else
		{
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
	  /// Starts the streaming message
	  /// 
	  /// <para>A streaming message starts with 'P'</para>
	  /// 
	  /// <pre>
	  /// P x02 x00
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeStreamingObject(Object obj) throws java.io.IOException
	  public virtual void writeStreamingObject(object obj)
	  {
		startStreamingPacket();

		writeObject(obj);

		endStreamingPacket();
	  }

	  /// <summary>
	  /// Starts a streaming packet
	  /// 
	  /// <para>A streaming message starts with 'P'</para>
	  /// 
	  /// <pre>
	  /// P x02 x00
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startStreamingPacket() throws java.io.IOException
	  public virtual void startStreamingPacket()
	  {
		if (_refs != null)
		{
		  _refs.clear();
		}

		flush();

		_isStreaming = true;
		_offset = 3;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void endStreamingPacket() throws java.io.IOException
	  public virtual void endStreamingPacket()
	  {
		int len = _offset - 3;

		_buffer[0] = (sbyte) 'P';
		_buffer[1] = (sbyte)(len >> 8);
		_buffer[2] = (sbyte) len;

		_isStreaming = false;

		flush();
	  }

	  /// <summary>
	  /// Prints a string to the stream, encoded as UTF-8 with preceeding length
	  /// </summary>
	  /// <param name="v"> the string to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void printLenString(String v) throws java.io.IOException
	  public virtual void printLenString(string v)
	  {
		if (SIZE < _offset + 16)
		{
		  flush();
		}

		if (string.ReferenceEquals(v, null))
		{
		  _buffer[_offset++] = (sbyte)(0);
		  _buffer[_offset++] = (sbyte)(0);
		}
		else
		{
		  int len = v.Length;
		  _buffer[_offset++] = (sbyte)(len >> 8);
		  _buffer[_offset++] = (sbyte)(len);

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
//ORIGINAL LINE: public void printString(String v, int strOffset, int length) throws java.io.IOException
	  public virtual void printString(string v, int strOffset, int length)
	  {
		int offset = _offset;
		sbyte[] buffer = _buffer;

		for (int i = 0; i < length; i++)
		{
		  if (SIZE <= offset + 16)
		  {
		_offset = offset;
		flush();
		offset = _offset;
		  }

		  char ch = v[i + strOffset];

		  if (ch < 0x80)
		  {
			buffer[offset++] = (sbyte)(ch);
		  }
		  else if (ch < 0x800)
		  {
			buffer[offset++] = unchecked((sbyte)(0xc0 + ((ch >> 6) & 0x1f)));
			buffer[offset++] = unchecked((sbyte)(0x80 + (ch & 0x3f)));
		  }
		  else
		  {
			buffer[offset++] = unchecked((sbyte)(0xe0 + ((ch >> 12) & 0xf)));
			buffer[offset++] = unchecked((sbyte)(0x80 + ((ch >> 6) & 0x3f)));
			buffer[offset++] = unchecked((sbyte)(0x80 + (ch & 0x3f)));
		  }
		}

		_offset = offset;
	  }

	  /// <summary>
	  /// Prints a string to the stream, encoded as UTF-8
	  /// </summary>
	  /// <param name="v"> the string to print. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void printString(char [] v, int strOffset, int length) throws java.io.IOException
	  public virtual void printString(char[] v, int strOffset, int length)
	  {
		int offset = _offset;
		sbyte[] buffer = _buffer;

		for (int i = 0; i < length; i++)
		{
		  if (SIZE <= offset + 16)
		  {
		_offset = offset;
		flush();
		offset = _offset;
		  }

		  char ch = v[i + strOffset];

		  if (ch < 0x80)
		  {
			buffer[offset++] = (sbyte)(ch);
		  }
		  else if (ch < 0x800)
		  {
			buffer[offset++] = unchecked((sbyte)(0xc0 + ((ch >> 6) & 0x1f)));
			buffer[offset++] = unchecked((sbyte)(0x80 + (ch & 0x3f)));
		  }
		  else
		  {
			buffer[offset++] = unchecked((sbyte)(0xe0 + ((ch >> 12) & 0xf)));
			buffer[offset++] = unchecked((sbyte)(0x80 + ((ch >> 6) & 0x3f)));
			buffer[offset++] = unchecked((sbyte)(0x80 + (ch & 0x3f)));
		  }
		}

		_offset = offset;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private final void flushIfFull() throws java.io.IOException
	  private void flushIfFull()
	  {
		int offset = _offset;

		if (SIZE < offset + 32)
		{
		  _offset = 0;
		  _os.Write(_buffer, 0, offset);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final void flush() throws java.io.IOException
	  public sealed override void flush()
	  {
		flushBuffer();

		if (_os != null)
		{
		  _os.Flush();
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final void flushBuffer() throws java.io.IOException
	  public void flushBuffer()
	  {
		int offset = _offset;

		if (!_isStreaming && offset > 0)
		{
		  _offset = 0;

		  _os.Write(_buffer, 0, offset);
		}
		else if (_isStreaming && offset > 3)
		{
		  int len = offset - 3;
		  _buffer[0] = (sbyte)'p';
		  _buffer[1] = (sbyte)(len >> 8);
		  _buffer[2] = (sbyte) len;
		  _offset = 3;

		  _os.Write(_buffer, 0, offset);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final void close() throws java.io.IOException
	  public sealed override void close()
	  {
		// hessian/3a8c
		flush();

		System.IO.Stream os = _os;
		_os = null;

		if (os != null)
		{
		  if (_isCloseStreamOnClose)
		  {
		os.Close();
		  }
		}
	  }

	  internal class BytesOutputStream : System.IO.Stream
	  {
		  private readonly Hessian2Output outerInstance;

		internal int _startOffset;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: BytesOutputStream() throws java.io.IOException
		internal BytesOutputStream(Hessian2Output outerInstance)
		{
			this.outerInstance = outerInstance;
		  if (SIZE < outerInstance._offset + 16)
		  {
			outerInstance.flush();
		  }

		  _startOffset = outerInstance._offset;
		  outerInstance._offset += 3; // skip 'b' xNN xNN
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(int ch) throws java.io.IOException
		public override void write(int ch)
		{
		  if (SIZE <= outerInstance._offset)
		  {
			int length = (outerInstance._offset - _startOffset) - 3;

			outerInstance._buffer[_startOffset] = (sbyte) Hessian2Constants_Fields.BC_BINARY_CHUNK;
			outerInstance._buffer[_startOffset + 1] = (sbyte)(length >> 8);
			outerInstance._buffer[_startOffset + 2] = (sbyte)(length);

			outerInstance.flush();

			_startOffset = outerInstance._offset;
			outerInstance._offset += 3;
		  }

		  outerInstance._buffer[outerInstance._offset++] = (sbyte) ch;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(byte [] buffer, int offset, int length) throws java.io.IOException
		public override void write(sbyte[] buffer, int offset, int length)
		{
		  while (length > 0)
		  {
			int sublen = SIZE - outerInstance._offset;

			if (length < sublen)
			{
			  sublen = length;
			}

			if (sublen > 0)
			{
			  Array.Copy(buffer, offset, outerInstance._buffer, outerInstance._offset, sublen);
			  outerInstance._offset += sublen;
			}

			length -= sublen;
			offset += sublen;

			if (SIZE <= outerInstance._offset)
			{
			  int chunkLength = (outerInstance._offset - _startOffset) - 3;

			  outerInstance._buffer[_startOffset] = (sbyte) Hessian2Constants_Fields.BC_BINARY_CHUNK;
			  outerInstance._buffer[_startOffset + 1] = (sbyte)(chunkLength >> 8);
			  outerInstance._buffer[_startOffset + 2] = (sbyte)(chunkLength);

			  outerInstance.flush();

			  _startOffset = outerInstance._offset;
			  outerInstance._offset += 3;
			}
		  }
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
		  int startOffset = _startOffset;
		  _startOffset = -1;

		  if (startOffset < 0)
		  {
			return;
		  }

		  int length = (outerInstance._offset - startOffset) - 3;

		  outerInstance._buffer[startOffset] = (sbyte) 'B';
		  outerInstance._buffer[startOffset + 1] = (sbyte)(length >> 8);
		  outerInstance._buffer[startOffset + 2] = (sbyte)(length);

		  outerInstance.flush();
		}
	  }
	}

}