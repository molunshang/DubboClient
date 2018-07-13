using System;
using System.Collections;
using System.Text;

/*
 * Copyright (c) 2001-2004 Caucho Technology, Inc.  All rights reserved.
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
 * 4. The names "Hessian", "Resin", and "Caucho" must not be used to
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
	/// Input stream for Hessian requests.
	/// 
	/// <para>HessianInput is unbuffered, so any client needs to provide
	/// its own buffering.
	/// 
	/// <pre>
	/// InputStream is = ...; // from http connection
	/// HessianInput in = new HessianInput(is);
	/// String value;
	/// 
	/// in.startReply();         // read reply header
	/// value = in.readString(); // read string value
	/// in.completeReply();      // read reply footer
	/// </pre>
	/// </para>
	/// </summary>
	public class HessianInput : AbstractHessianInput
	{
	  private static int END_OF_DATA = -2;

	  private static Field _detailMessageField;

	  // factory for deserializing objects in the input stream
	  protected internal SerializerFactory _serializerFactory;

	  protected internal ArrayList _refs;

	  // the underlying input stream
	  private System.IO.Stream _is;
	  // a peek character
	  protected internal int _peek = -1;

	  // the method for a call
	  private string _method;

	  private Reader _chunkReader;
	  private System.IO.Stream _chunkInputStream;

	  private Exception _replyFault;

	  private StringBuilder _sbuf = new StringBuilder();

	  // true if this is the last chunk
	  private bool _isLastChunk;
	  // the chunk length
	  private int _chunkLength;

	  /// <summary>
	  /// Creates an uninitialized Hessian input stream.
	  /// </summary>
	  public HessianInput()
	  {
	  }

	  /// <summary>
	  /// Creates a new Hessian input stream, initialized with an
	  /// underlying input stream.
	  /// </summary>
	  /// <param name="is"> the underlying input stream. </param>
	  public HessianInput(System.IO.Stream @is)
	  {
		init(@is);
	  }

	  /// <summary>
	  /// Sets the serializer factory.
	  /// </summary>
	  public override SerializerFactory SerializerFactory
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
	  /// Initialize the hessian stream with the underlying input stream.
	  /// </summary>
	  public override void init(System.IO.Stream @is)
	  {
		_is = @is;
		_method = null;
		_isLastChunk = true;
		_chunkLength = 0;
		_peek = -1;
		_refs = null;
		_replyFault = null;

		if (_serializerFactory == null)
		{
		  _serializerFactory = new SerializerFactory();
		}
	  }

	  /// <summary>
	  /// Returns the calls method
	  /// </summary>
	  public override string Method
	  {
		  get
		  {
			return _method;
		  }
	  }

	  /// <summary>
	  /// Returns any reply fault.
	  /// </summary>
	  public virtual Exception ReplyFault
	  {
		  get
		  {
			return _replyFault;
		  }
	  }

	  /// <summary>
	  /// Starts reading the call
	  /// 
	  /// <pre>
	  /// c major minor
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readCall() throws java.io.IOException
	  public override int readCall()
	  {
		int tag = read();

		if (tag != 'c')
		{
		  throw error("expected hessian call ('c') at " + codeName(tag));
		}

		int major = read();
		int minor = read();

		return (major << 16) + minor;
	  }

	  /// <summary>
	  /// For backward compatibility with HessianSkeleton
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void skipOptionalCall() throws java.io.IOException
	  public override void skipOptionalCall()
	  {
		int tag = read();

		if (tag == 'c')
		{
		  read();
		  read();
		}
		else
		{
		  _peek = tag;
		}
	  }

	  /// <summary>
	  /// Starts reading the call
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// m b16 b8 method
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readMethod() throws java.io.IOException
	  public override string readMethod()
	  {
		int tag = read();

		if (tag != 'm')
		{
		  throw error("expected hessian method ('m') at " + codeName(tag));
		}
		int d1 = read();
		int d2 = read();

		_isLastChunk = true;
		_chunkLength = d1 * 256 + d2;
		_sbuf.Length = 0;
		int ch;
		while ((ch = parseChar()) >= 0)
		{
		  _sbuf.Append((char) ch);
		}

		_method = _sbuf.ToString();

		return _method;
	  }

	  /// <summary>
	  /// Starts reading the call, including the headers.
	  /// 
	  /// <para>The call expects the following protocol data
	  /// 
	  /// <pre>
	  /// c major minor
	  /// m b16 b8 method
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startCall() throws java.io.IOException
	  public override void startCall()
	  {
		readCall();

		while (!string.ReferenceEquals(readHeader(), null))
		{
		  readObject();
		}

		readMethod();
	  }

	  /// <summary>
	  /// Completes reading the call
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// z
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeCall() throws java.io.IOException
	  public override void completeCall()
	  {
		int tag = read();

		if (tag == 'z')
		{
		}
		else
		{
		  throw error("expected end of call ('z') at " + codeName(tag) + ".  Check method arguments and ensure method overloading is enabled if necessary");
		}
	  }

	  /// <summary>
	  /// Reads a reply as an object.
	  /// If the reply has a fault, throws the exception.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readReply(Class expectedClass) throws Throwable
	  public override object readReply(Type expectedClass)
	  {
		int tag = read();

		if (tag != 'r')
		{
		  error("expected hessian reply at " + codeName(tag));
		}

		int major = read();
		int minor = read();

		tag = read();
		if (tag == 'f')
		{
		  throw prepareFault();
		}
		else
		{
		  _peek = tag;

		  object value = readObject(expectedClass);

		  completeValueReply();

		  return value;
		}
	  }

	  /// <summary>
	  /// Starts reading the reply
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// r
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startReply() throws Throwable
	  public override void startReply()
	  {
		int tag = read();

		if (tag != 'r')
		{
		  error("expected hessian reply at " + codeName(tag));
		}

		int major = read();
		int minor = read();

		tag = read();
		if (tag == 'f')
		{
		  throw prepareFault();
		}
		else
		{
		  _peek = tag;
		}
	  }

	  /// <summary>
	  /// Prepares the fault.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Throwable prepareFault() throws java.io.IOException
	  private Exception prepareFault()
	  {
		Hashtable fault = readFault();

		object detail = fault["detail"];
		string message = (string) fault["message"];

		if (detail is Exception)
		{
		  _replyFault = (Exception) detail;

		  if (!string.ReferenceEquals(message, null) && _detailMessageField != null)
		  {
		try
		{
		  _detailMessageField.set(_replyFault, message);
		}
		catch (Exception)
		{
		}
		  }

		  return _replyFault;
		}

		else
		{
		  string code = (string) fault["code"];

		  _replyFault = new HessianServiceException(message, code, detail);

		  return _replyFault;
		}
	  }

	  /// <summary>
	  /// Completes reading the call
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
		int tag = read();

		if (tag != 'z')
		{
		  error("expected end of reply at " + codeName(tag));
		}
	  }

	  /// <summary>
	  /// Completes reading the call
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// z
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeValueReply() throws java.io.IOException
	  public virtual void completeValueReply()
	  {
		int tag = read();

		if (tag != 'z')
		{
		  error("expected end of reply at " + codeName(tag));
		}
	  }

	  /// <summary>
	  /// Reads a header, returning null if there are no headers.
	  /// 
	  /// <pre>
	  /// H b16 b8 value
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readHeader() throws java.io.IOException
	  public override string readHeader()
	  {
		int tag = read();

		if (tag == 'H')
		{
		  _isLastChunk = true;
		  _chunkLength = (read() << 8) + read();

		  _sbuf.Length = 0;
		  int ch;
		  while ((ch = parseChar()) >= 0)
		  {
			_sbuf.Append((char) ch);
		  }

		  return _sbuf.ToString();
		}

		_peek = tag;

		return null;
	  }

	  /// <summary>
	  /// Reads a null
	  /// 
	  /// <pre>
	  /// N
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readNull() throws java.io.IOException
	  public override void readNull()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
			return;

		default:
		  throw expect("null", tag);
		}
	  }

	  /// <summary>
	  /// Reads a boolean
	  /// 
	  /// <pre>
	  /// T
	  /// F
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean readBoolean() throws java.io.IOException
	  public override bool readBoolean()
	  {
		int tag = read();

		switch (tag)
		{
		case 'T':
			return true;
		case 'F':
			return false;
		case 'I':
			return parseInt() == 0;
		case 'L':
			return parseLong() == 0;
		case 'D':
			return parseDouble() == 0.0;
		case 'N':
			return false;

		default:
		  throw expect("boolean", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte
	  /// 
	  /// <pre>
	  /// I b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
	  /*
	  public byte readByte()
	    throws IOException
	  {
	    return (byte) readInt();
	  }
	  */

	  /// <summary>
	  /// Reads a short
	  /// 
	  /// <pre>
	  /// I b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public short readShort() throws java.io.IOException
	  public virtual short readShort()
	  {
		return (short) readInt();
	  }

	  /// <summary>
	  /// Reads an integer
	  /// 
	  /// <pre>
	  /// I b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readInt() throws java.io.IOException
	  public override int readInt()
	  {
		int tag = read();

		switch (tag)
		{
		case 'T':
			return 1;
		case 'F':
			return 0;
		case 'I':
			return parseInt();
		case 'L':
			return (int) parseLong();
		case 'D':
			return (int) parseDouble();

		default:
		  throw expect("int", tag);
		}
	  }

	  /// <summary>
	  /// Reads a long
	  /// 
	  /// <pre>
	  /// L b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long readLong() throws java.io.IOException
	  public override long readLong()
	  {
		int tag = read();

		switch (tag)
		{
		case 'T':
			return 1;
		case 'F':
			return 0;
		case 'I':
			return parseInt();
		case 'L':
			return parseLong();
		case 'D':
			return (long) parseDouble();

		default:
		  throw expect("long", tag);
		}
	  }

	  /// <summary>
	  /// Reads a float
	  /// 
	  /// <pre>
	  /// D b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public float readFloat() throws java.io.IOException
	  public virtual float readFloat()
	  {
		return (float) readDouble();
	  }

	  /// <summary>
	  /// Reads a double
	  /// 
	  /// <pre>
	  /// D b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double readDouble() throws java.io.IOException
	  public override double readDouble()
	  {
		int tag = read();

		switch (tag)
		{
		case 'T':
			return 1;
		case 'F':
			return 0;
		case 'I':
			return parseInt();
		case 'L':
			return (double) parseLong();
		case 'D':
			return parseDouble();

		default:
		  throw expect("long", tag);
		}
	  }

	  /// <summary>
	  /// Reads a date.
	  /// 
	  /// <pre>
	  /// T b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long readUTCDate() throws java.io.IOException
	  public override long readUTCDate()
	  {
		int tag = read();

		if (tag != 'd')
		{
		  throw error("expected date at " + codeName(tag));
		}

		long b64 = read();
		long b56 = read();
		long b48 = read();
		long b40 = read();
		long b32 = read();
		long b24 = read();
		long b16 = read();
		long b8 = read();

		return ((b64 << 56) + (b56 << 48) + (b48 << 40) + (b40 << 32) + (b32 << 24) + (b24 << 16) + (b16 << 8) + b8);
	  }

	  /// <summary>
	  /// Reads a byte from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readChar() throws java.io.IOException
	  public virtual int readChar()
	  {
		if (_chunkLength > 0)
		{
		  _chunkLength--;
		  if (_chunkLength == 0 && _isLastChunk)
		  {
			_chunkLength = END_OF_DATA;
		  }

		  int ch = parseUTF8Char();
		  return ch;
		}
		else if (_chunkLength == END_OF_DATA)
		{
		  _chunkLength = 0;
		  return -1;
		}

		int tag = read();

		switch (tag)
		{
		case 'N':
		  return -1;

		case 'S':
		case 's':
		case 'X':
		case 'x':
		  _isLastChunk = tag == 'S' || tag == 'X';
		  _chunkLength = (read() << 8) + read();

		  _chunkLength--;
		  int value = parseUTF8Char();

		  // special code so successive read byte won't
		  // be read as a single object.
		  if (_chunkLength == 0 && _isLastChunk)
		  {
			_chunkLength = END_OF_DATA;
		  }

		  return value;

		default:
		  throw new IOException("expected 'S' at " + (char) tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte array from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readString(char [] buffer, int offset, int length) throws java.io.IOException
	  public virtual int readString(char[] buffer, int offset, int length)
	  {
		int readLength = 0;

		if (_chunkLength == END_OF_DATA)
		{
		  _chunkLength = 0;
		  return -1;
		}
		else if (_chunkLength == 0)
		{
		  int tag = read();

		  switch (tag)
		  {
		  case 'N':
			return -1;

		  case 'S':
		  case 's':
		  case 'X':
		  case 'x':
			_isLastChunk = tag == 'S' || tag == 'X';
			_chunkLength = (read() << 8) + read();
			break;

		  default:
			throw new IOException("expected 'S' at " + (char) tag);
		  }
		}

		while (length > 0)
		{
		  if (_chunkLength > 0)
		  {
			buffer[offset++] = (char) parseUTF8Char();
			_chunkLength--;
			length--;
			readLength++;
		  }
		  else if (_isLastChunk)
		  {
			if (readLength == 0)
			{
			  return -1;
			}
			else
			{
			  _chunkLength = END_OF_DATA;
			  return readLength;
			}
		  }
		  else
		  {
			int tag = read();

			switch (tag)
			{
			case 'S':
			case 's':
			case 'X':
			case 'x':
			  _isLastChunk = tag == 'S' || tag == 'X';
			  _chunkLength = (read() << 8) + read();
			  break;

			default:
			  throw new IOException("expected 'S' at " + (char) tag);
			}
		  }
		}

		if (readLength == 0)
		{
		  return -1;
		}
		else if (_chunkLength > 0 || !_isLastChunk)
		{
		  return readLength;
		}
		else
		{
		  _chunkLength = END_OF_DATA;
		  return readLength;
		}
	  }

	  /// <summary>
	  /// Reads a string
	  /// 
	  /// <pre>
	  /// S b16 b8 string value
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readString() throws java.io.IOException
	  public override string readString()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'I':
		  return parseInt().ToString();
		case 'L':
		  return parseLong().ToString();
		case 'D':
		  return parseDouble().ToString();

		case 'S':
		case 's':
		case 'X':
		case 'x':
		  _isLastChunk = tag == 'S' || tag == 'X';
		  _chunkLength = (read() << 8) + read();

		  _sbuf.Length = 0;
		  int ch;

		  while ((ch = parseChar()) >= 0)
		  {
			_sbuf.Append((char) ch);
		  }

		  return _sbuf.ToString();

		default:
		  throw expect("string", tag);
		}
	  }

	  /// <summary>
	  /// Reads an XML node.
	  /// 
	  /// <pre>
	  /// S b16 b8 string value
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.w3c.dom.Node readNode() throws java.io.IOException
	  public override org.w3c.dom.Node readNode()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'S':
		case 's':
		case 'X':
		case 'x':
		  _isLastChunk = tag == 'S' || tag == 'X';
		  _chunkLength = (read() << 8) + read();

		  throw error("Can't handle string in this context");

		default:
		  throw expect("string", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte array
	  /// 
	  /// <pre>
	  /// B b16 b8 data value
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte [] readBytes() throws java.io.IOException
	  public override sbyte [] readBytes()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'B':
		case 'b':
		  _isLastChunk = tag == 'B';
		  _chunkLength = (read() << 8) + read();

		  System.IO.MemoryStream bos = new System.IO.MemoryStream();

		  int data;
		  while ((data = parseByte()) >= 0)
		  {
			bos.WriteByte(data);
		  }

		  return bos.toByteArray();

		default:
		  throw expect("bytes", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readByte() throws java.io.IOException
	  public virtual int readByte()
	  {
		if (_chunkLength > 0)
		{
		  _chunkLength--;
		  if (_chunkLength == 0 && _isLastChunk)
		  {
			_chunkLength = END_OF_DATA;
		  }

		  return read();
		}
		else if (_chunkLength == END_OF_DATA)
		{
		  _chunkLength = 0;
		  return -1;
		}

		int tag = read();

		switch (tag)
		{
		case 'N':
		  return -1;

		case 'B':
		case 'b':
		  _isLastChunk = tag == 'B';
		  _chunkLength = (read() << 8) + read();

		  int value = parseByte();

		  // special code so successive read byte won't
		  // be read as a single object.
		  if (_chunkLength == 0 && _isLastChunk)
		  {
			_chunkLength = END_OF_DATA;
		  }

		  return value;

		default:
		  throw new IOException("expected 'B' at " + (char) tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte array from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readBytes(byte [] buffer, int offset, int length) throws java.io.IOException
	  public virtual int readBytes(sbyte[] buffer, int offset, int length)
	  {
		int readLength = 0;

		if (_chunkLength == END_OF_DATA)
		{
		  _chunkLength = 0;
		  return -1;
		}
		else if (_chunkLength == 0)
		{
		  int tag = read();

		  switch (tag)
		  {
		  case 'N':
			return -1;

		  case 'B':
		  case 'b':
			_isLastChunk = tag == 'B';
			_chunkLength = (read() << 8) + read();
			break;

		  default:
			throw new IOException("expected 'B' at " + (char) tag);
		  }
		}

		while (length > 0)
		{
		  if (_chunkLength > 0)
		  {
			buffer[offset++] = (sbyte) read();
			_chunkLength--;
			length--;
			readLength++;
		  }
		  else if (_isLastChunk)
		  {
			if (readLength == 0)
			{
			  return -1;
			}
			else
			{
			  _chunkLength = END_OF_DATA;
			  return readLength;
			}
		  }
		  else
		  {
			int tag = read();

			switch (tag)
			{
			case 'B':
			case 'b':
			  _isLastChunk = tag == 'B';
			  _chunkLength = (read() << 8) + read();
			  break;

			default:
			  throw new IOException("expected 'B' at " + (char) tag);
			}
		  }
		}

		if (readLength == 0)
		{
		  return -1;
		}
		else if (_chunkLength > 0 || !_isLastChunk)
		{
		  return readLength;
		}
		else
		{
		  _chunkLength = END_OF_DATA;
		  return readLength;
		}
	  }

	  /// <summary>
	  /// Reads a fault.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.HashMap readFault() throws java.io.IOException
	  private Hashtable readFault()
	  {
		Hashtable map = new Hashtable();

		int code = read();
		for (; code > 0 && code != 'z'; code = read())
		{
		  _peek = code;

		  object key = readObject();
		  object value = readObject();

		  if (key != null && value != null)
		  {
			map[key] = value;
		  }
		}

		if (code != 'z')
		{
		  throw expect("fault", code);
		}

		return map;
	  }

	  /// <summary>
	  /// Reads an object from the input stream with an expected type.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(Class cl) throws java.io.IOException
	  public override object readObject(Type cl)
	  {
		if (cl == null || cl == typeof(object))
		{
		  return readObject();
		}

		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'M':
		{
		  string type = readType();

		  // hessian/3386
		  if ("".Equals(type))
		  {
		Deserializer reader;
		reader = _serializerFactory.getDeserializer(cl);

		return reader.readMap(this);
		  }
		  else
		  {
		Deserializer reader;
		reader = _serializerFactory.getObjectDeserializer(type,cl);

			return reader.readMap(this);
		  }
		}

			goto case 'V';
		case 'V':
		{
		  string type = readType();
		  int length = readLength();

		  Deserializer reader;
		  reader = _serializerFactory.getObjectDeserializer(type);

		  if (cl != reader.Type && cl.IsAssignableFrom(reader.Type))
		  {
			return reader.readList(this, length);
		  }

		  reader = _serializerFactory.getDeserializer(cl);

		  object v = reader.readList(this, length);

		  return v;
		}

		case 'R':
		{
		  int @ref = parseInt();

		  return _refs[@ref];
		}

		case 'r':
		{
		  string type = readType();
		  string url = readString();

		  return resolveRemote(type, url);
		}
		}

		_peek = tag;

		// hessian/332i vs hessian/3406
		//return readObject();

		object value = _serializerFactory.getDeserializer(cl).readObject(this);

		return value;
	  }

	  /// <summary>
	  /// Reads an arbitrary object from the input stream when the type
	  /// is unknown.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject() throws java.io.IOException
	  public override object readObject()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'T':
		  return Convert.ToBoolean(true);

		case 'F':
		  return Convert.ToBoolean(false);

		case 'I':
		  return Convert.ToInt32(parseInt());

		case 'L':
		  return Convert.ToInt64(parseLong());

		case 'D':
		  return Convert.ToDouble(parseDouble());

		case 'd':
		  return new DateTime(parseLong());

		case 'x':
		case 'X':
		{
		  _isLastChunk = tag == 'X';
		  _chunkLength = (read() << 8) + read();

		  return parseXML();
		}

		case 's':
		case 'S':
		{
		  _isLastChunk = tag == 'S';
		  _chunkLength = (read() << 8) + read();

		  int data;
		  _sbuf.Length = 0;

		  while ((data = parseChar()) >= 0)
		  {
			_sbuf.Append((char) data);
		  }

		  return _sbuf.ToString();
		}

		case 'b':
		case 'B':
		{
		  _isLastChunk = tag == 'B';
		  _chunkLength = (read() << 8) + read();

		  int data;
		  System.IO.MemoryStream bos = new System.IO.MemoryStream();

		  while ((data = parseByte()) >= 0)
		  {
			bos.WriteByte(data);
		  }

		  return bos.toByteArray();
		}

		case 'V':
		{
		  string type = readType();
		  int length = readLength();

		  return _serializerFactory.readList(this, length, type);
		}

		case 'M':
		{
		  string type = readType();

		  return _serializerFactory.readMap(this, type);
		}

		case 'R':
		{
		  int @ref = parseInt();

		  return _refs[@ref];
		}

		case 'r':
		{
		  string type = readType();
		  string url = readString();

		  return resolveRemote(type, url);
		}

		default:
		  throw error("unknown code for readObject at " + codeName(tag));
		}
	  }

	  /// <summary>
	  /// Reads a remote object.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readRemote() throws java.io.IOException
	  public override object readRemote()
	  {
		string type = readType();
		string url = readString();

		return resolveRemote(type, url);
	  }

	  /// <summary>
	  /// Reads a reference.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readRef() throws java.io.IOException
	  public override object readRef()
	  {
		return _refs[parseInt()];
	  }

	  /// <summary>
	  /// Reads the start of a list.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readListStart() throws java.io.IOException
	  public override int readListStart()
	  {
		return read();
	  }

	  /// <summary>
	  /// Reads the start of a list.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readMapStart() throws java.io.IOException
	  public override int readMapStart()
	  {
		return read();
	  }

	  /// <summary>
	  /// Returns true if this is the end of a list or a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isEnd() throws java.io.IOException
	  public override bool End
	  {
		  get
		  {
			int code = read();
    
			_peek = code;
    
			return (code < 0 || code == 'z');
		  }
	  }

	  /// <summary>
	  /// Reads the end byte.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readEnd() throws java.io.IOException
	  public override void readEnd()
	  {
		int code = read();

		if (code != 'z')
		{
		  throw error("unknown code at " + codeName(code));
		}
	  }

	  /// <summary>
	  /// Reads the end byte.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readMapEnd() throws java.io.IOException
	  public override void readMapEnd()
	  {
		int code = read();

		if (code != 'z')
		{
		  throw error("expected end of map ('z') at " + codeName(code));
		}
	  }

	  /// <summary>
	  /// Reads the end byte.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readListEnd() throws java.io.IOException
	  public override void readListEnd()
	  {
		int code = read();

		if (code != 'z')
		{
		  throw error("expected end of list ('z') at " + codeName(code));
		}
	  }

	  /// <summary>
	  /// Adds a list/map reference.
	  /// </summary>
	  public override int addRef(object @ref)
	  {
		if (_refs == null)
		{
		  _refs = new ArrayList();
		}

		_refs.Add(@ref);

		return _refs.Count - 1;
	  }

	  /// <summary>
	  /// Adds a list/map reference.
	  /// </summary>
	  public override void setRef(int i, object @ref)
	  {
		_refs[i] = @ref;
	  }

	  /// <summary>
	  /// Resets the references for streaming.
	  /// </summary>
	  public override void resetReferences()
	  {
		if (_refs != null)
		{
		  _refs.Clear();
		}
	  }

	  /// <summary>
	  /// Resolves a remote object.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object resolveRemote(String type, String url) throws java.io.IOException
	  public virtual object resolveRemote(string type, string url)
	  {
		HessianRemoteResolver resolver = RemoteResolver;

		if (resolver != null)
		{
		  return resolver.lookup(type, url);
		}
		else
		{
		  return new HessianRemote(type, url);
		}
	  }

	  /// <summary>
	  /// Parses a type from the stream.
	  /// 
	  /// <pre>
	  /// t b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readType() throws java.io.IOException
	  public override string readType()
	  {
		int code = read();

		if (code != 't')
		{
		  _peek = code;
		  return "";
		}

		_isLastChunk = true;
		_chunkLength = (read() << 8) + read();

		_sbuf.Length = 0;
		int ch;
		while ((ch = parseChar()) >= 0)
		{
		  _sbuf.Append((char) ch);
		}

		return _sbuf.ToString();
	  }

	  /// <summary>
	  /// Parses the length for an array
	  /// 
	  /// <pre>
	  /// l b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readLength() throws java.io.IOException
	  public override int readLength()
	  {
		int code = read();

		if (code != 'l')
		{
		  _peek = code;
		  return -1;
		}

		return parseInt();
	  }

	  /// <summary>
	  /// Parses a 32-bit integer value from the stream.
	  /// 
	  /// <pre>
	  /// b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int parseInt() throws java.io.IOException
	  private int parseInt()
	  {
		int b32 = read();
		int b24 = read();
		int b16 = read();
		int b8 = read();

		return (b32 << 24) + (b24 << 16) + (b16 << 8) + b8;
	  }

	  /// <summary>
	  /// Parses a 64-bit long value from the stream.
	  /// 
	  /// <pre>
	  /// b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private long parseLong() throws java.io.IOException
	  private long parseLong()
	  {
		long b64 = read();
		long b56 = read();
		long b48 = read();
		long b40 = read();
		long b32 = read();
		long b24 = read();
		long b16 = read();
		long b8 = read();

		return ((b64 << 56) + (b56 << 48) + (b48 << 40) + (b40 << 32) + (b32 << 24) + (b24 << 16) + (b16 << 8) + b8);
	  }

	  /// <summary>
	  /// Parses a 64-bit double value from the stream.
	  /// 
	  /// <pre>
	  /// b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private double parseDouble() throws java.io.IOException
	  private double parseDouble()
	  {
		long b64 = read();
		long b56 = read();
		long b48 = read();
		long b40 = read();
		long b32 = read();
		long b24 = read();
		long b16 = read();
		long b8 = read();

		long bits = ((b64 << 56) + (b56 << 48) + (b48 << 40) + (b40 << 32) + (b32 << 24) + (b24 << 16) + (b16 << 8) + b8);

		return Double.longBitsToDouble(bits);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: org.w3c.dom.Node parseXML() throws java.io.IOException
	  internal virtual org.w3c.dom.Node parseXML()
	  {
		throw new System.NotSupportedException();
	  }

	  /// <summary>
	  /// Reads a character from the underlying stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int parseChar() throws java.io.IOException
	  private int parseChar()
	  {
		while (_chunkLength <= 0)
		{
		  if (_isLastChunk)
		  {
			return -1;
		  }

		  int code = read();

		  switch (code)
		  {
		  case 's':
		  case 'x':
			_isLastChunk = false;

			_chunkLength = (read() << 8) + read();
			break;

		  case 'S':
		  case 'X':
			_isLastChunk = true;

			_chunkLength = (read() << 8) + read();
			break;

		  default:
			throw expect("string", code);
		  }

		}

		_chunkLength--;

		return parseUTF8Char();
	  }

	  /// <summary>
	  /// Parses a single UTF8 character.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int parseUTF8Char() throws java.io.IOException
	  private int parseUTF8Char()
	  {
		int ch = read();

		if (ch < 0x80)
		{
		  return ch;
		}
		else if ((ch & 0xe0) == 0xc0)
		{
		  int ch1 = read();
		  int v = ((ch & 0x1f) << 6) + (ch1 & 0x3f);

		  return v;
		}
		else if ((ch & 0xf0) == 0xe0)
		{
		  int ch1 = read();
		  int ch2 = read();
		  int v = ((ch & 0x0f) << 12) + ((ch1 & 0x3f) << 6) + (ch2 & 0x3f);

		  return v;
		}
		else
		{
		  throw error("bad utf-8 encoding at " + codeName(ch));
		}
	  }

	  /// <summary>
	  /// Reads a byte from the underlying stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int parseByte() throws java.io.IOException
	  private int parseByte()
	  {
		while (_chunkLength <= 0)
		{
		  if (_isLastChunk)
		  {
			return -1;
		  }

		  int code = read();

		  switch (code)
		  {
		  case 'b':
			_isLastChunk = false;

			_chunkLength = (read() << 8) + read();
			break;

		  case 'B':
			_isLastChunk = true;

			_chunkLength = (read() << 8) + read();
			break;

		  default:
			throw expect("byte[]", code);
		  }
		}

		_chunkLength--;

		return read();
	  }

	  /// <summary>
	  /// Reads bytes based on an input stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.InputStream readInputStream() throws java.io.IOException
	  public override System.IO.Stream readInputStream()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'B':
		case 'b':
		  _isLastChunk = tag == 'B';
		  _chunkLength = (read() << 8) + read();
		  break;

		default:
		  throw expect("inputStream", tag);
		}

		return new InputStreamAnonymousInnerClass(this);
	  }

	  private class InputStreamAnonymousInnerClass : System.IO.Stream
	  {
		  private readonly HessianInput outerInstance;

		  public InputStreamAnonymousInnerClass(HessianInput outerInstance)
		  {
			  this.outerInstance = outerInstance;
			  _isClosed = false;
		  }

		  internal bool _isClosed;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read() throws java.io.IOException
		  public virtual int read()
		  {
			if (_isClosed || outerInstance._is == null)
			{
			  return -1;
			}

			int ch = outerInstance.parseByte();
			if (ch < 0)
			{
			  _isClosed = true;
			}

			return ch;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read(byte [] buffer, int offset, int length) throws java.io.IOException
		  public virtual int read(sbyte[] buffer, int offset, int length)
		  {
			if (_isClosed || outerInstance._is == null)
			{
			  return -1;
			}

			int len = outerInstance.read(buffer, offset, length);
			if (len < 0)
			{
			  _isClosed = true;
			}

			return len;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		  public virtual void close()
		  {
			while (outerInstance.read() >= 0)
			{
			}

			_isClosed = true;
		  }
	  }

	  /// <summary>
	  /// Reads bytes from the underlying stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int read(byte [] buffer, int offset, int length) throws java.io.IOException
	  internal virtual int read(sbyte[] buffer, int offset, int length)
	  {
		int readLength = 0;

		while (length > 0)
		{
		  while (_chunkLength <= 0)
		  {
			if (_isLastChunk)
			{
			  return readLength == 0 ? - 1 : readLength;
			}

			int code = read();

			switch (code)
			{
			case 'b':
			  _isLastChunk = false;

			  _chunkLength = (read() << 8) + read();
			  break;

			case 'B':
			  _isLastChunk = true;

			  _chunkLength = (read() << 8) + read();
			  break;

			default:
			  throw expect("byte[]", code);
			}
		  }

		  int sublen = _chunkLength;
		  if (length < sublen)
		  {
			sublen = length;
		  }

		  sublen = _is.Read(buffer, offset, sublen);
		  offset += sublen;
		  readLength += sublen;
		  length -= sublen;
		  _chunkLength -= sublen;
		}

		return readLength;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: final int read() throws java.io.IOException
	  internal int read()
	  {
		if (_peek >= 0)
		{
		  int value = _peek;
		  _peek = -1;
		  return value;
		}

		int ch = _is.Read();

		return ch;
	  }

	  public override void close()
	  {
		_is = null;
	  }

	  public override Reader Reader
	  {
		  get
		  {
			return null;
		  }
	  }

	  protected internal virtual IOException expect(string expect, int ch)
	  {
		return error("expected " + expect + " at " + codeName(ch));
	  }

	  protected internal virtual string codeName(int ch)
	  {
		if (ch < 0)
		{
		  return "end of file";
		}
		else
		{
		  return "0x" + (ch & 0xff).ToString("x") + " (" + (char) + ch + ")";
		}
	  }

	  protected internal virtual IOException error(string message)
	  {
		if (!string.ReferenceEquals(_method, null))
		{
		  return new HessianProtocolException(_method + ": " + message);
		}
		else
		{
		  return new HessianProtocolException(message);
		}
	  }

	  static HessianInput()
	  {
		try
		{
		  _detailMessageField = typeof(Exception).getDeclaredField("detailMessage");
		  _detailMessageField.Accessible = true;
		}
		catch (Exception)
		{
		}
	  }
	}

}