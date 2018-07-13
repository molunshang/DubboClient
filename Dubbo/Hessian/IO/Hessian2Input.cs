using System;
using System.Collections;
using System.Text;

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
	public class Hessian2Input : AbstractHessianInput, Hessian2Constants
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
	  private static readonly Logger log = Logger.getLogger(typeof(Hessian2Input).FullName);

	  private const double D_256 = 1.0 / 256.0;
	  private const int END_OF_DATA = -2;

	  private static Field _detailMessageField;

	  private const int SIZE = 256;
	  private const int GAP = 16;

	  // factory for deserializing objects in the input stream
	  protected internal SerializerFactory _serializerFactory;

	  private static bool _isCloseStreamOnClose;

	  protected internal ArrayList _refs;
	  protected internal ArrayList _classDefs;
	  protected internal ArrayList _types;

	  // the underlying input stream
	  private System.IO.Stream _is;
	  private readonly sbyte[] _buffer = new sbyte[SIZE];

	  // a peek character
	  private int _offset;
	  private int _length;

	  // true for streaming data
	  private bool _isStreaming;

	  // the method for a call
	  private string _method;
	  private int _argLength;

	  private Reader _chunkReader;
	  private System.IO.Stream _chunkInputStream;

	  private Exception _replyFault;

	  private StringBuilder _sbuf = new StringBuilder();

	  // true if this is the last chunk
	  private bool _isLastChunk;
	  // the chunk length
	  private int _chunkLength;

	  /// <summary>
	  /// Creates a new Hessian input stream, initialized with an
	  /// underlying input stream.
	  /// </summary>
	  /// <param name="is"> the underlying input stream. </param>
	  public Hessian2Input(System.IO.Stream @is)
	  {
		_is = @is;
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
	  /// Gets the serializer factory, creating a default if necessary.
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
//ORIGINAL LINE: public int readCall() throws IOException
	  public override int readCall()
	  {
		int tag = read();

		if (tag != 'C')
		{
		  throw error("expected hessian call ('C') at " + codeName(tag));
		}

		return 0;
	  }

	  /// <summary>
	  /// Starts reading the envelope
	  /// 
	  /// <pre>
	  /// E major minor
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readEnvelope() throws IOException
	  public virtual int readEnvelope()
	  {
		int tag = read();
		int version = 0;

		if (tag == 'H')
		{
		  int major = read();
		  int minor = read();

		  version = (major << 16) + minor;

		  tag = read();
		}

		if (tag != 'E')
		{
		  throw error("expected hessian Envelope ('E') at " + codeName(tag));
		}

		return version;
	  }

	  /// <summary>
	  /// Completes reading the envelope
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// Z
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeEnvelope() throws IOException
	  public virtual void completeEnvelope()
	  {
		int tag = read();

		if (tag != 'Z')
		{
		  error("expected end of envelope at " + codeName(tag));
		}
	  }

	  /// <summary>
	  /// Starts reading the call
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// string
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readMethod() throws IOException
	  public override string readMethod()
	  {
		_method = readString();

		return _method;
	  }

	  /// <summary>
	  /// Returns the number of method arguments
	  /// 
	  /// <pre>
	  /// int
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int readMethodArgLength() throws IOException
	  public override int readMethodArgLength()
	  {
		return readInt();
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
//ORIGINAL LINE: public void startCall() throws IOException
	  public override void startCall()
	  {
		readCall();

		readMethod();
	  }

	  /// <summary>
	  /// Completes reading the call
	  /// 
	  /// <para>A successful completion will have a single value:
	  /// 
	  /// <pre>
	  /// </pre>
	  /// </para>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void completeCall() throws IOException
	  public override void completeCall()
	  {
	  }

	  /// <summary>
	  /// Reads a reply as an object.
	  /// If the reply has a fault, throws the exception.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object readReply(Class expectedClass) throws Throwable
	  public override object readReply(Type expectedClass)
	  {
		int tag = read();

		if (tag == 'R')
		{
		  return readObject(expectedClass);
		}
		else if (tag == 'F')
		{
		  Hashtable map = (Hashtable) readObject(typeof(Hashtable));

		  throw prepareFault(map);
		}
		else
		{
		  StringBuilder sb = new StringBuilder();
		  sb.Append((char) tag);

		  try
		  {
		int ch;

		while ((ch = read()) >= 0)
		{
		  sb.Append((char) ch);
		}
		  }
		  catch (IOException e)
		  {
		log.log(Level.FINE, e.ToString(), e);
		  }

		  throw error("expected hessian reply at " + codeName(tag) + "\n" + sb);
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
		// XXX: for variable length (?)

		readReply(typeof(object));
	  }

	  /// <summary>
	  /// Prepares the fault.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Throwable prepareFault(java.util.HashMap fault) throws IOException
	  private Exception prepareFault(Hashtable fault)
	  {
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
//ORIGINAL LINE: public void completeReply() throws IOException
	  public override void completeReply()
	  {
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
//ORIGINAL LINE: public void completeValueReply() throws IOException
	  public virtual void completeValueReply()
	  {
		int tag = read();

		if (tag != 'Z')
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
//ORIGINAL LINE: public String readHeader() throws IOException
	  public override string readHeader()
	  {
		return null;
	  }

	  /// <summary>
	  /// Starts reading the message
	  /// 
	  /// <pre>
	  /// p major minor
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int startMessage() throws IOException
	  public virtual int startMessage()
	  {
		int tag = read();

		if (tag == 'p')
		{
		  _isStreaming = false;
		}
		else if (tag == 'P')
		{
		  _isStreaming = true;
		}
		else
		{
		  throw error("expected Hessian message ('p') at " + codeName(tag));
		}

		int major = read();
		int minor = read();

		return (major << 16) + minor;
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
//ORIGINAL LINE: public void completeMessage() throws IOException
	  public virtual void completeMessage()
	  {
		int tag = read();

		if (tag != 'Z')
		{
		  error("expected end of message at " + codeName(tag));
		}
	  }

	  /// <summary>
	  /// Reads a null
	  /// 
	  /// <pre>
	  /// N
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readNull() throws IOException
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
//ORIGINAL LINE: public boolean readBoolean() throws IOException
	  public override bool readBoolean()
	  {
		int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		switch (tag)
		{
		case 'T':
			return true;
		case 'F':
			return false;

		  // direct integer
		case 0x80:
	case 0x81:
	case 0x82:
	case 0x83:
		case 0x84:
	case 0x85:
	case 0x86:
	case 0x87:
		case 0x88:
	case 0x89:
	case 0x8a:
	case 0x8b:
		case 0x8c:
	case 0x8d:
	case 0x8e:
	case 0x8f:

		case 0x90:
	case 0x91:
	case 0x92:
	case 0x93:
		case 0x94:
	case 0x95:
	case 0x96:
	case 0x97:
		case 0x98:
	case 0x99:
	case 0x9a:
	case 0x9b:
		case 0x9c:
	case 0x9d:
	case 0x9e:
	case 0x9f:

		case 0xa0:
	case 0xa1:
	case 0xa2:
	case 0xa3:
		case 0xa4:
	case 0xa5:
	case 0xa6:
	case 0xa7:
		case 0xa8:
	case 0xa9:
	case 0xaa:
	case 0xab:
		case 0xac:
	case 0xad:
	case 0xae:
	case 0xaf:

		case 0xb0:
	case 0xb1:
	case 0xb2:
	case 0xb3:
		case 0xb4:
	case 0xb5:
	case 0xb6:
	case 0xb7:
		case 0xb8:
	case 0xb9:
	case 0xba:
	case 0xbb:
		case 0xbc:
	case 0xbd:
	case 0xbe:
	case 0xbf:
		  return tag != Hessian2Constants_Fields.BC_INT_ZERO;

		  // INT_BYTE = 0
		case 0xc8:
		  return read() != 0;

		  // INT_BYTE != 0
		case 0xc0:
	case 0xc1:
	case 0xc2:
	case 0xc3:
		case 0xc4:
	case 0xc5:
	case 0xc6:
	case 0xc7:
		case 0xc9:
	case 0xca:
	case 0xcb:
		case 0xcc:
	case 0xcd:
	case 0xce:
	case 0xcf:
		  read();
		  return true;

		  // INT_SHORT = 0
		case 0xd4:
		  return (256 * read() + read()) != 0;

		  // INT_SHORT != 0
		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd5:
	case 0xd6:
	case 0xd7:
		  read();
		  read();
		  return true;

		case 'I':
			return parseInt() != 0;

		case 0xd8:
	case 0xd9:
	case 0xda:
	case 0xdb:
		case 0xdc:
	case 0xdd:
	case 0xde:
	case 0xdf:

		case 0xe0:
	case 0xe1:
	case 0xe2:
	case 0xe3:
		case 0xe4:
	case 0xe5:
	case 0xe6:
	case 0xe7:
		case 0xe8:
	case 0xe9:
	case 0xea:
	case 0xeb:
		case 0xec:
	case 0xed:
	case 0xee:
	case 0xef:
		  return tag != Hessian2Constants_Fields.BC_LONG_ZERO;

		  // LONG_BYTE = 0
		case 0xf8:
		  return read() != 0;

		  // LONG_BYTE != 0
		case 0xf0:
	case 0xf1:
	case 0xf2:
	case 0xf3:
		case 0xf4:
	case 0xf5:
	case 0xf6:
	case 0xf7:
		case 0xf9:
	case 0xfa:
	case 0xfb:
		case 0xfc:
	case 0xfd:
	case 0xfe:
	case 0xff:
		  read();
		  return true;

		  // INT_SHORT = 0
		case 0x3c:
		  return (256 * read() + read()) != 0;

		  // INT_SHORT != 0
		case 0x38:
	case 0x39:
	case 0x3a:
	case 0x3b:
		case 0x3d:
	case 0x3e:
	case 0x3f:
		  read();
		  read();
		  return true;

		case Hessian2Constants_Fields.BC_LONG_INT:
		  return (0x1000000L * read() + 0x10000L * read() + 0x100 * read() + read()) != 0;

		case 'L':
		  return parseLong() != 0;

		case Hessian2Constants_Fields.BC_DOUBLE_ZERO:
		  return false;

		case Hessian2Constants_Fields.BC_DOUBLE_ONE:
		  return true;

		case Hessian2Constants_Fields.BC_DOUBLE_BYTE:
		  return read() != 0;

		case Hessian2Constants_Fields.BC_DOUBLE_SHORT:
		  return (0x100 * read() + read()) != 0;

		case Hessian2Constants_Fields.BC_DOUBLE_MILL:
		{
		int mills = parseInt();

		return mills != 0;
		}

		case 'D':
		  return parseDouble() != 0.0;

		case 'N':
		  return false;

		default:
		  throw expect("boolean", tag);
		}
	  }

	  /// <summary>
	  /// Reads a short
	  /// 
	  /// <pre>
	  /// I b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public short readShort() throws IOException
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
//ORIGINAL LINE: public final int readInt() throws IOException
	  public sealed override int readInt()
	  {
		//int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : read();
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return 0;

		case 'F':
		  return 0;

		case 'T':
		  return 1;

		  // direct integer
		case 0x80:
	case 0x81:
	case 0x82:
	case 0x83:
		case 0x84:
	case 0x85:
	case 0x86:
	case 0x87:
		case 0x88:
	case 0x89:
	case 0x8a:
	case 0x8b:
		case 0x8c:
	case 0x8d:
	case 0x8e:
	case 0x8f:

		case 0x90:
	case 0x91:
	case 0x92:
	case 0x93:
		case 0x94:
	case 0x95:
	case 0x96:
	case 0x97:
		case 0x98:
	case 0x99:
	case 0x9a:
	case 0x9b:
		case 0x9c:
	case 0x9d:
	case 0x9e:
	case 0x9f:

		case 0xa0:
	case 0xa1:
	case 0xa2:
	case 0xa3:
		case 0xa4:
	case 0xa5:
	case 0xa6:
	case 0xa7:
		case 0xa8:
	case 0xa9:
	case 0xaa:
	case 0xab:
		case 0xac:
	case 0xad:
	case 0xae:
	case 0xaf:

		case 0xb0:
	case 0xb1:
	case 0xb2:
	case 0xb3:
		case 0xb4:
	case 0xb5:
	case 0xb6:
	case 0xb7:
		case 0xb8:
	case 0xb9:
	case 0xba:
	case 0xbb:
		case 0xbc:
	case 0xbd:
	case 0xbe:
	case 0xbf:
		  return tag - Hessian2Constants_Fields.BC_INT_ZERO;

		  /* byte int */
		case 0xc0:
	case 0xc1:
	case 0xc2:
	case 0xc3:
		case 0xc4:
	case 0xc5:
	case 0xc6:
	case 0xc7:
		case 0xc8:
	case 0xc9:
	case 0xca:
	case 0xcb:
		case 0xcc:
	case 0xcd:
	case 0xce:
	case 0xcf:
		  return ((tag - Hessian2Constants_Fields.BC_INT_BYTE_ZERO) << 8) + read();

		  /* short int */
		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd4:
	case 0xd5:
	case 0xd6:
	case 0xd7:
		  return ((tag - Hessian2Constants_Fields.BC_INT_SHORT_ZERO) << 16) + 256 * read() + read();

		case 'I':
		case Hessian2Constants_Fields.BC_LONG_INT:
		  return ((read() << 24) + (read() << 16) + (read() << 8) + read());

		  // direct long
		case 0xd8:
	case 0xd9:
	case 0xda:
	case 0xdb:
		case 0xdc:
	case 0xdd:
	case 0xde:
	case 0xdf:

		case 0xe0:
	case 0xe1:
	case 0xe2:
	case 0xe3:
		case 0xe4:
	case 0xe5:
	case 0xe6:
	case 0xe7:
		case 0xe8:
	case 0xe9:
	case 0xea:
	case 0xeb:
		case 0xec:
	case 0xed:
	case 0xee:
	case 0xef:
		  return tag - Hessian2Constants_Fields.BC_LONG_ZERO;

		  /* byte long */
		case 0xf0:
	case 0xf1:
	case 0xf2:
	case 0xf3:
		case 0xf4:
	case 0xf5:
	case 0xf6:
	case 0xf7:
		case 0xf8:
	case 0xf9:
	case 0xfa:
	case 0xfb:
		case 0xfc:
	case 0xfd:
	case 0xfe:
	case 0xff:
		  return ((tag - Hessian2Constants_Fields.BC_LONG_BYTE_ZERO) << 8) + read();

		  /* short long */
		case 0x38:
	case 0x39:
	case 0x3a:
	case 0x3b:
		case 0x3c:
	case 0x3d:
	case 0x3e:
	case 0x3f:
		  return ((tag - Hessian2Constants_Fields.BC_LONG_SHORT_ZERO) << 16) + 256 * read() + read();

		case 'L':
		  return (int) parseLong();

		case Hessian2Constants_Fields.BC_DOUBLE_ZERO:
		  return 0;

		case Hessian2Constants_Fields.BC_DOUBLE_ONE:
		  return 1;

		  //case LONG_BYTE:
		case Hessian2Constants_Fields.BC_DOUBLE_BYTE:
		  return (sbyte)(_offset < _length ? _buffer[_offset++] : read());

		  //case INT_SHORT:
		  //case LONG_SHORT:
		case Hessian2Constants_Fields.BC_DOUBLE_SHORT:
		  return (short)(256 * read() + read());

		case Hessian2Constants_Fields.BC_DOUBLE_MILL:
		{
		int mills = parseInt();

		return (int)(0.001 * mills);
		}

		case 'D':
		  return (int) parseDouble();

		default:
		  throw expect("integer", tag);
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
//ORIGINAL LINE: public long readLong() throws IOException
	  public override long readLong()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return 0;

		case 'F':
		  return 0;

		case 'T':
		  return 1;

		  // direct integer
		case 0x80:
	case 0x81:
	case 0x82:
	case 0x83:
		case 0x84:
	case 0x85:
	case 0x86:
	case 0x87:
		case 0x88:
	case 0x89:
	case 0x8a:
	case 0x8b:
		case 0x8c:
	case 0x8d:
	case 0x8e:
	case 0x8f:

		case 0x90:
	case 0x91:
	case 0x92:
	case 0x93:
		case 0x94:
	case 0x95:
	case 0x96:
	case 0x97:
		case 0x98:
	case 0x99:
	case 0x9a:
	case 0x9b:
		case 0x9c:
	case 0x9d:
	case 0x9e:
	case 0x9f:

		case 0xa0:
	case 0xa1:
	case 0xa2:
	case 0xa3:
		case 0xa4:
	case 0xa5:
	case 0xa6:
	case 0xa7:
		case 0xa8:
	case 0xa9:
	case 0xaa:
	case 0xab:
		case 0xac:
	case 0xad:
	case 0xae:
	case 0xaf:

		case 0xb0:
	case 0xb1:
	case 0xb2:
	case 0xb3:
		case 0xb4:
	case 0xb5:
	case 0xb6:
	case 0xb7:
		case 0xb8:
	case 0xb9:
	case 0xba:
	case 0xbb:
		case 0xbc:
	case 0xbd:
	case 0xbe:
	case 0xbf:
		  return tag - Hessian2Constants_Fields.BC_INT_ZERO;

		  /* byte int */
		case 0xc0:
	case 0xc1:
	case 0xc2:
	case 0xc3:
		case 0xc4:
	case 0xc5:
	case 0xc6:
	case 0xc7:
		case 0xc8:
	case 0xc9:
	case 0xca:
	case 0xcb:
		case 0xcc:
	case 0xcd:
	case 0xce:
	case 0xcf:
		  return ((tag - Hessian2Constants_Fields.BC_INT_BYTE_ZERO) << 8) + read();

		  /* short int */
		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd4:
	case 0xd5:
	case 0xd6:
	case 0xd7:
		  return ((tag - Hessian2Constants_Fields.BC_INT_SHORT_ZERO) << 16) + 256 * read() + read();

		  //case LONG_BYTE:
		case Hessian2Constants_Fields.BC_DOUBLE_BYTE:
		  return (sbyte)(_offset < _length ? _buffer[_offset++] : read());

		  //case INT_SHORT:
		  //case LONG_SHORT:
		case Hessian2Constants_Fields.BC_DOUBLE_SHORT:
		  return (short)(256 * read() + read());

		case 'I':
		case Hessian2Constants_Fields.BC_LONG_INT:
		  return parseInt();

		  // direct long
		case 0xd8:
	case 0xd9:
	case 0xda:
	case 0xdb:
		case 0xdc:
	case 0xdd:
	case 0xde:
	case 0xdf:

		case 0xe0:
	case 0xe1:
	case 0xe2:
	case 0xe3:
		case 0xe4:
	case 0xe5:
	case 0xe6:
	case 0xe7:
		case 0xe8:
	case 0xe9:
	case 0xea:
	case 0xeb:
		case 0xec:
	case 0xed:
	case 0xee:
	case 0xef:
		  return tag - Hessian2Constants_Fields.BC_LONG_ZERO;

		  /* byte long */
		case 0xf0:
	case 0xf1:
	case 0xf2:
	case 0xf3:
		case 0xf4:
	case 0xf5:
	case 0xf6:
	case 0xf7:
		case 0xf8:
	case 0xf9:
	case 0xfa:
	case 0xfb:
		case 0xfc:
	case 0xfd:
	case 0xfe:
	case 0xff:
		  return ((tag - Hessian2Constants_Fields.BC_LONG_BYTE_ZERO) << 8) + read();

		  /* short long */
		case 0x38:
	case 0x39:
	case 0x3a:
	case 0x3b:
		case 0x3c:
	case 0x3d:
	case 0x3e:
	case 0x3f:
		  return ((tag - Hessian2Constants_Fields.BC_LONG_SHORT_ZERO) << 16) + 256 * read() + read();

		case 'L':
		  return parseLong();

		case Hessian2Constants_Fields.BC_DOUBLE_ZERO:
		  return 0;

		case Hessian2Constants_Fields.BC_DOUBLE_ONE:
		  return 1;

		case Hessian2Constants_Fields.BC_DOUBLE_MILL:
		{
		int mills = parseInt();

		return (long)(0.001 * mills);
		}

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
//ORIGINAL LINE: public float readFloat() throws IOException
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
//ORIGINAL LINE: public double readDouble() throws IOException
	  public override double readDouble()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return 0;

		case 'F':
		  return 0;

		case 'T':
		  return 1;

		  // direct integer
		case 0x80:
	case 0x81:
	case 0x82:
	case 0x83:
		case 0x84:
	case 0x85:
	case 0x86:
	case 0x87:
		case 0x88:
	case 0x89:
	case 0x8a:
	case 0x8b:
		case 0x8c:
	case 0x8d:
	case 0x8e:
	case 0x8f:

		case 0x90:
	case 0x91:
	case 0x92:
	case 0x93:
		case 0x94:
	case 0x95:
	case 0x96:
	case 0x97:
		case 0x98:
	case 0x99:
	case 0x9a:
	case 0x9b:
		case 0x9c:
	case 0x9d:
	case 0x9e:
	case 0x9f:

		case 0xa0:
	case 0xa1:
	case 0xa2:
	case 0xa3:
		case 0xa4:
	case 0xa5:
	case 0xa6:
	case 0xa7:
		case 0xa8:
	case 0xa9:
	case 0xaa:
	case 0xab:
		case 0xac:
	case 0xad:
	case 0xae:
	case 0xaf:

		case 0xb0:
	case 0xb1:
	case 0xb2:
	case 0xb3:
		case 0xb4:
	case 0xb5:
	case 0xb6:
	case 0xb7:
		case 0xb8:
	case 0xb9:
	case 0xba:
	case 0xbb:
		case 0xbc:
	case 0xbd:
	case 0xbe:
	case 0xbf:
		  return tag - 0x90;

		  /* byte int */
		case 0xc0:
	case 0xc1:
	case 0xc2:
	case 0xc3:
		case 0xc4:
	case 0xc5:
	case 0xc6:
	case 0xc7:
		case 0xc8:
	case 0xc9:
	case 0xca:
	case 0xcb:
		case 0xcc:
	case 0xcd:
	case 0xce:
	case 0xcf:
		  return ((tag - Hessian2Constants_Fields.BC_INT_BYTE_ZERO) << 8) + read();

		  /* short int */
		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd4:
	case 0xd5:
	case 0xd6:
	case 0xd7:
		  return ((tag - Hessian2Constants_Fields.BC_INT_SHORT_ZERO) << 16) + 256 * read() + read();

		case 'I':
		case Hessian2Constants_Fields.BC_LONG_INT:
		  return parseInt();

		  // direct long
		case 0xd8:
	case 0xd9:
	case 0xda:
	case 0xdb:
		case 0xdc:
	case 0xdd:
	case 0xde:
	case 0xdf:

		case 0xe0:
	case 0xe1:
	case 0xe2:
	case 0xe3:
		case 0xe4:
	case 0xe5:
	case 0xe6:
	case 0xe7:
		case 0xe8:
	case 0xe9:
	case 0xea:
	case 0xeb:
		case 0xec:
	case 0xed:
	case 0xee:
	case 0xef:
		  return tag - Hessian2Constants_Fields.BC_LONG_ZERO;

		  /* byte long */
		case 0xf0:
	case 0xf1:
	case 0xf2:
	case 0xf3:
		case 0xf4:
	case 0xf5:
	case 0xf6:
	case 0xf7:
		case 0xf8:
	case 0xf9:
	case 0xfa:
	case 0xfb:
		case 0xfc:
	case 0xfd:
	case 0xfe:
	case 0xff:
		  return ((tag - Hessian2Constants_Fields.BC_LONG_BYTE_ZERO) << 8) + read();

		  /* short long */
		case 0x38:
	case 0x39:
	case 0x3a:
	case 0x3b:
		case 0x3c:
	case 0x3d:
	case 0x3e:
	case 0x3f:
		  return ((tag - Hessian2Constants_Fields.BC_LONG_SHORT_ZERO) << 16) + 256 * read() + read();

		case 'L':
		  return (double) parseLong();

		case Hessian2Constants_Fields.BC_DOUBLE_ZERO:
		  return 0;

		case Hessian2Constants_Fields.BC_DOUBLE_ONE:
		  return 1;

		case Hessian2Constants_Fields.BC_DOUBLE_BYTE:
		  return (sbyte)(_offset < _length ? _buffer[_offset++] : read());

		case Hessian2Constants_Fields.BC_DOUBLE_SHORT:
		  return (short)(256 * read() + read());

		case Hessian2Constants_Fields.BC_DOUBLE_MILL:
		{
		int mills = parseInt();

		return 0.001 * mills;
		}

		case 'D':
		  return parseDouble();

		default:
		  throw expect("double", tag);
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
//ORIGINAL LINE: public long readUTCDate() throws IOException
	  public override long readUTCDate()
	  {
		int tag = read();

		if (tag == Hessian2Constants_Fields.BC_DATE)
		{
		  return parseLong();
		}
		else if (tag == Hessian2Constants_Fields.BC_DATE_MINUTE)
		{
		  return parseInt() * 60000L;
		}
		else
		{
		  throw expect("date", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readChar() throws IOException
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
		case Hessian2Constants_Fields.BC_STRING_CHUNK:
		  _isLastChunk = tag == 'S';
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
		  throw expect("char", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte array from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readString(char [] buffer, int offset, int length) throws IOException
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
		  case Hessian2Constants_Fields.BC_STRING_CHUNK:
			_isLastChunk = tag == 'S';
			_chunkLength = (read() << 8) + read();
			break;

		  case 0x00:
	  case 0x01:
	case 0x02:
	case 0x03:
		  case 0x04:
	  case 0x05:
	case 0x06:
	case 0x07:
		  case 0x08:
	  case 0x09:
	case 0x0a:
	case 0x0b:
		  case 0x0c:
	  case 0x0d:
	case 0x0e:
	case 0x0f:

		  case 0x10:
	  case 0x11:
	case 0x12:
	case 0x13:
		  case 0x14:
	  case 0x15:
	case 0x16:
	case 0x17:
		  case 0x18:
	  case 0x19:
	case 0x1a:
	case 0x1b:
		  case 0x1c:
	  case 0x1d:
	case 0x1e:
	case 0x1f:
		_isLastChunk = true;
		_chunkLength = tag - 0x00;
		break;

		  default:
			throw expect("string", tag);
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
			case Hessian2Constants_Fields.BC_STRING_CHUNK:
			  _isLastChunk = tag == 'S';
			  _chunkLength = (read() << 8) + read();
			  break;

			default:
			  throw expect("string", tag);
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
//ORIGINAL LINE: public String readString() throws IOException
	  public override string readString()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;
		case 'T':
		  return "true";
		case 'F':
		  return "false";

		  // direct integer
		case 0x80:
	case 0x81:
	case 0x82:
	case 0x83:
		case 0x84:
	case 0x85:
	case 0x86:
	case 0x87:
		case 0x88:
	case 0x89:
	case 0x8a:
	case 0x8b:
		case 0x8c:
	case 0x8d:
	case 0x8e:
	case 0x8f:

		case 0x90:
	case 0x91:
	case 0x92:
	case 0x93:
		case 0x94:
	case 0x95:
	case 0x96:
	case 0x97:
		case 0x98:
	case 0x99:
	case 0x9a:
	case 0x9b:
		case 0x9c:
	case 0x9d:
	case 0x9e:
	case 0x9f:

		case 0xa0:
	case 0xa1:
	case 0xa2:
	case 0xa3:
		case 0xa4:
	case 0xa5:
	case 0xa6:
	case 0xa7:
		case 0xa8:
	case 0xa9:
	case 0xaa:
	case 0xab:
		case 0xac:
	case 0xad:
	case 0xae:
	case 0xaf:

		case 0xb0:
	case 0xb1:
	case 0xb2:
	case 0xb3:
		case 0xb4:
	case 0xb5:
	case 0xb6:
	case 0xb7:
		case 0xb8:
	case 0xb9:
	case 0xba:
	case 0xbb:
		case 0xbc:
	case 0xbd:
	case 0xbe:
	case 0xbf:
		  return (tag - 0x90).ToString();

		  /* byte int */
		case 0xc0:
	case 0xc1:
	case 0xc2:
	case 0xc3:
		case 0xc4:
	case 0xc5:
	case 0xc6:
	case 0xc7:
		case 0xc8:
	case 0xc9:
	case 0xca:
	case 0xcb:
		case 0xcc:
	case 0xcd:
	case 0xce:
	case 0xcf:
		  return (((tag - Hessian2Constants_Fields.BC_INT_BYTE_ZERO) << 8) + read()).ToString();

		  /* short int */
		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd4:
	case 0xd5:
	case 0xd6:
	case 0xd7:
		  return (((tag - Hessian2Constants_Fields.BC_INT_SHORT_ZERO) << 16) + 256 * read() + read()).ToString();

		case 'I':
		case Hessian2Constants_Fields.BC_LONG_INT:
		  return parseInt().ToString();

		  // direct long
		case 0xd8:
	case 0xd9:
	case 0xda:
	case 0xdb:
		case 0xdc:
	case 0xdd:
	case 0xde:
	case 0xdf:

		case 0xe0:
	case 0xe1:
	case 0xe2:
	case 0xe3:
		case 0xe4:
	case 0xe5:
	case 0xe6:
	case 0xe7:
		case 0xe8:
	case 0xe9:
	case 0xea:
	case 0xeb:
		case 0xec:
	case 0xed:
	case 0xee:
	case 0xef:
		  return (tag - Hessian2Constants_Fields.BC_LONG_ZERO).ToString();

		  /* byte long */
		case 0xf0:
	case 0xf1:
	case 0xf2:
	case 0xf3:
		case 0xf4:
	case 0xf5:
	case 0xf6:
	case 0xf7:
		case 0xf8:
	case 0xf9:
	case 0xfa:
	case 0xfb:
		case 0xfc:
	case 0xfd:
	case 0xfe:
	case 0xff:
		  return (((tag - Hessian2Constants_Fields.BC_LONG_BYTE_ZERO) << 8) + read()).ToString();

		  /* short long */
		case 0x38:
	case 0x39:
	case 0x3a:
	case 0x3b:
		case 0x3c:
	case 0x3d:
	case 0x3e:
	case 0x3f:
		  return (((tag - Hessian2Constants_Fields.BC_LONG_SHORT_ZERO) << 16) + 256 * read() + read()).ToString();

		case 'L':
		  return parseLong().ToString();

		case Hessian2Constants_Fields.BC_DOUBLE_ZERO:
		  return "0.0";

		case Hessian2Constants_Fields.BC_DOUBLE_ONE:
		  return "1.0";

		case Hessian2Constants_Fields.BC_DOUBLE_BYTE:
		  return ((sbyte)(_offset < _length ? _buffer[_offset++] : read())).ToString();

		case Hessian2Constants_Fields.BC_DOUBLE_SHORT:
		  return ((short)(256 * read() + read())).ToString();

		case Hessian2Constants_Fields.BC_DOUBLE_MILL:
		{
		int mills = parseInt();

		return (0.001 * mills).ToString();
		}

		case 'D':
		  return parseDouble().ToString();

		case 'S':
		case Hessian2Constants_Fields.BC_STRING_CHUNK:
		  _isLastChunk = tag == 'S';
		  _chunkLength = (read() << 8) + read();

		  _sbuf.Length = 0;
		  int ch;

		  while ((ch = parseChar()) >= 0)
		  {
			_sbuf.Append((char) ch);
		  }

		  return _sbuf.ToString();

		  // 0-byte string
		case 0x00:
	case 0x01:
	case 0x02:
	case 0x03:
		case 0x04:
	case 0x05:
	case 0x06:
	case 0x07:
		case 0x08:
	case 0x09:
	case 0x0a:
	case 0x0b:
		case 0x0c:
	case 0x0d:
	case 0x0e:
	case 0x0f:

		case 0x10:
	case 0x11:
	case 0x12:
	case 0x13:
		case 0x14:
	case 0x15:
	case 0x16:
	case 0x17:
		case 0x18:
	case 0x19:
	case 0x1a:
	case 0x1b:
		case 0x1c:
	case 0x1d:
	case 0x1e:
	case 0x1f:
		  _isLastChunk = true;
		  _chunkLength = tag - 0x00;

		  _sbuf.Length = 0;

		  while ((ch = parseChar()) >= 0)
		  {
			_sbuf.Append((char) ch);
		  }

		  return _sbuf.ToString();

		case 0x30:
	case 0x31:
	case 0x32:
	case 0x33:
		  _isLastChunk = true;
		  _chunkLength = (tag - 0x30) * 256 + read();

		  _sbuf.Length = 0;

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
	  /// Reads a byte array
	  /// 
	  /// <pre>
	  /// B b16 b8 data value
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte [] readBytes() throws IOException
	  public override sbyte [] readBytes()
	  {
		int tag = read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'B':
		case Hessian2Constants_Fields.BC_BINARY_CHUNK:
		  _isLastChunk = tag == 'B';
		  _chunkLength = (read() << 8) + read();

		  System.IO.MemoryStream bos = new System.IO.MemoryStream();

		  int data;
		  while ((data = parseByte()) >= 0)
		  {
			bos.WriteByte(data);
		  }

		  return bos.toByteArray();

		case 0x20:
	case 0x21:
	case 0x22:
	case 0x23:
		case 0x24:
	case 0x25:
	case 0x26:
	case 0x27:
		case 0x28:
	case 0x29:
	case 0x2a:
	case 0x2b:
		case 0x2c:
	case 0x2d:
	case 0x2e:
	case 0x2f:
	{
		_isLastChunk = true;
		_chunkLength = tag - 0x20;

		sbyte[] buffer = new sbyte[_chunkLength];

		int k = 0;
		while ((data = parseByte()) >= 0)
		{
		  buffer[k++] = (sbyte) data;
		}

		return buffer;
	}

		case 0x34:
	case 0x35:
	case 0x36:
	case 0x37:
	{
		_isLastChunk = true;
		_chunkLength = (tag - 0x34) * 256 + read();

		sbyte[] buffer = new sbyte[_chunkLength];
		int k = 0;

		while ((data = parseByte()) >= 0)
		{
		  buffer[k++] = (sbyte) data;
		}

		return buffer;
	}

		default:
		  throw expect("bytes", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readByte() throws IOException
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
		case Hessian2Constants_Fields.BC_BINARY_CHUNK:
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
		  throw expect("binary", tag);
		}
	  }

	  /// <summary>
	  /// Reads a byte array from the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readBytes(byte [] buffer, int offset, int length) throws IOException
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
		  case Hessian2Constants_Fields.BC_BINARY_CHUNK:
			_isLastChunk = tag == 'B';
			_chunkLength = (read() << 8) + read();
			break;

		  default:
			throw expect("binary", tag);
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
			case Hessian2Constants_Fields.BC_BINARY_CHUNK:
			  _isLastChunk = tag == 'B';
			  _chunkLength = (read() << 8) + read();
			  break;

			default:
			  throw expect("binary", tag);
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
//ORIGINAL LINE: private java.util.HashMap readFault() throws IOException
	  private Hashtable readFault()
	  {
		Hashtable map = new Hashtable();

		int code = read();
		for (; code > 0 && code != 'Z'; code = read())
		{
		  _offset--;

		  object key = readObject();
		  object value = readObject();

		  if (key != null && value != null)
		  {
			map[key] = value;
		  }
		}

		if (code != 'Z')
		{
		  throw expect("fault", code);
		}

		return map;
	  }

	  /// <summary>
	  /// Reads an object from the input stream with an expected type.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(Class cl) throws IOException
	  public override object readObject(Type cl)
	  {
		if (cl == null || cl == typeof(object))
		{
		  return readObject();
		}

		int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'H':
		{
		Deserializer reader = findSerializerFactory().getDeserializer(cl);

		return reader.readMap(this);
		}

		case 'M':
		{
		string type = readType();

		// hessian/3bb3
		if ("".Equals(type))
		{
		  Deserializer reader;
		  reader = findSerializerFactory().getDeserializer(cl);

		  return reader.readMap(this);
		}
		else
		{
		  Deserializer reader;
		  reader = findSerializerFactory().getObjectDeserializer(type, cl);

		  return reader.readMap(this);
		}
		}

			goto case 'C';
		case 'C':
		{
		readObjectDefinition(cl);

		return readObject(cl);
		}

		case 0x60:
	case 0x61:
	case 0x62:
	case 0x63:
		case 0x64:
	case 0x65:
	case 0x66:
	case 0x67:
		case 0x68:
	case 0x69:
	case 0x6a:
	case 0x6b:
		case 0x6c:
	case 0x6d:
	case 0x6e:
	case 0x6f:
	{
		int @ref = tag - 0x60;
		int size = _classDefs.Count;

		if (@ref < 0 || size <= @ref)
		{
		  throw new HessianProtocolException("'" + @ref + "' is an unknown class definition");
		}

		ObjectDefinition def = (ObjectDefinition) _classDefs[@ref];

		return readObjectInstance(cl, def);
	}

		case 'O':
		{
		int @ref = readInt();
		int size = _classDefs.Count;

		if (@ref < 0 || size <= @ref)
		{
		  throw new HessianProtocolException("'" + @ref + "' is an unknown class definition");
		}

		ObjectDefinition def = (ObjectDefinition) _classDefs[@ref];

		return readObjectInstance(cl, def);
		}

		case Hessian2Constants_Fields.BC_LIST_VARIABLE:
		{
		string type = readType();

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(type, cl);

		object v = reader.readList(this, -1);

		return v;
		}

		case Hessian2Constants_Fields.BC_LIST_FIXED:
		{
		string type = readType();
		int length = readInt();

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(type, cl);

		object v = reader.readLengthList(this, length);

		return v;
		}

		case 0x70:
	case 0x71:
	case 0x72:
	case 0x73:
		case 0x74:
	case 0x75:
	case 0x76:
	case 0x77:
	{
		int length = tag - 0x70;

		string type = readType();

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(null, cl);

		object v = reader.readLengthList(this, length);

		return v;
	}

		case Hessian2Constants_Fields.BC_LIST_VARIABLE_UNTYPED:
		{
		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(null, cl);

		object v = reader.readList(this, -1);

		return v;
		}

		case Hessian2Constants_Fields.BC_LIST_FIXED_UNTYPED:
		{
		int length = readInt();

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(null, cl);

		object v = reader.readLengthList(this, length);

		return v;
		}

		case 0x78:
	case 0x79:
	case 0x7a:
	case 0x7b:
		case 0x7c:
	case 0x7d:
	case 0x7e:
	case 0x7f:
	{
		int length = tag - 0x78;

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(null, cl);

		object v = reader.readLengthList(this, length);

		return v;
	}

		case Hessian2Constants_Fields.BC_REF:
		{
		int @ref = readInt();

		return _refs[@ref];
		}
		}

		if (tag >= 0)
		{
		  _offset--;
		}

		// hessian/3b2i vs hessian/3406
		// return readObject();
		object value = findSerializerFactory().getDeserializer(cl).readObject(this);
		return value;
	  }

	  /// <summary>
	  /// Reads an arbitrary object from the input stream when the type
	  /// is unknown.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject() throws IOException
	  public override object readObject()
	  {
		int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		switch (tag)
		{
		case 'N':
		  return null;

		case 'T':
		  return Convert.ToBoolean(true);

		case 'F':
		  return Convert.ToBoolean(false);

		  // direct integer
		case 0x80:
	case 0x81:
	case 0x82:
	case 0x83:
		case 0x84:
	case 0x85:
	case 0x86:
	case 0x87:
		case 0x88:
	case 0x89:
	case 0x8a:
	case 0x8b:
		case 0x8c:
	case 0x8d:
	case 0x8e:
	case 0x8f:

		case 0x90:
	case 0x91:
	case 0x92:
	case 0x93:
		case 0x94:
	case 0x95:
	case 0x96:
	case 0x97:
		case 0x98:
	case 0x99:
	case 0x9a:
	case 0x9b:
		case 0x9c:
	case 0x9d:
	case 0x9e:
	case 0x9f:

		case 0xa0:
	case 0xa1:
	case 0xa2:
	case 0xa3:
		case 0xa4:
	case 0xa5:
	case 0xa6:
	case 0xa7:
		case 0xa8:
	case 0xa9:
	case 0xaa:
	case 0xab:
		case 0xac:
	case 0xad:
	case 0xae:
	case 0xaf:

		case 0xb0:
	case 0xb1:
	case 0xb2:
	case 0xb3:
		case 0xb4:
	case 0xb5:
	case 0xb6:
	case 0xb7:
		case 0xb8:
	case 0xb9:
	case 0xba:
	case 0xbb:
		case 0xbc:
	case 0xbd:
	case 0xbe:
	case 0xbf:
		  return Convert.ToInt32(tag - Hessian2Constants_Fields.BC_INT_ZERO);

		  /* byte int */
		case 0xc0:
	case 0xc1:
	case 0xc2:
	case 0xc3:
		case 0xc4:
	case 0xc5:
	case 0xc6:
	case 0xc7:
		case 0xc8:
	case 0xc9:
	case 0xca:
	case 0xcb:
		case 0xcc:
	case 0xcd:
	case 0xce:
	case 0xcf:
		  return Convert.ToInt32(((tag - Hessian2Constants_Fields.BC_INT_BYTE_ZERO) << 8) + read());

		  /* short int */
		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd4:
	case 0xd5:
	case 0xd6:
	case 0xd7:
		  return Convert.ToInt32(((tag - Hessian2Constants_Fields.BC_INT_SHORT_ZERO) << 16) + 256 * read() + read());

		case 'I':
		  return Convert.ToInt32(parseInt());

		  // direct long
		case 0xd8:
	case 0xd9:
	case 0xda:
	case 0xdb:
		case 0xdc:
	case 0xdd:
	case 0xde:
	case 0xdf:

		case 0xe0:
	case 0xe1:
	case 0xe2:
	case 0xe3:
		case 0xe4:
	case 0xe5:
	case 0xe6:
	case 0xe7:
		case 0xe8:
	case 0xe9:
	case 0xea:
	case 0xeb:
		case 0xec:
	case 0xed:
	case 0xee:
	case 0xef:
		  return Convert.ToInt64(tag - Hessian2Constants_Fields.BC_LONG_ZERO);

		  /* byte long */
		case 0xf0:
	case 0xf1:
	case 0xf2:
	case 0xf3:
		case 0xf4:
	case 0xf5:
	case 0xf6:
	case 0xf7:
		case 0xf8:
	case 0xf9:
	case 0xfa:
	case 0xfb:
		case 0xfc:
	case 0xfd:
	case 0xfe:
	case 0xff:
		  return Convert.ToInt64(((tag - Hessian2Constants_Fields.BC_LONG_BYTE_ZERO) << 8) + read());

		  /* short long */
		case 0x38:
	case 0x39:
	case 0x3a:
	case 0x3b:
		case 0x3c:
	case 0x3d:
	case 0x3e:
	case 0x3f:
		  return Convert.ToInt64(((tag - Hessian2Constants_Fields.BC_LONG_SHORT_ZERO) << 16) + 256 * read() + read());

		case Hessian2Constants_Fields.BC_LONG_INT:
		  return Convert.ToInt64(parseInt());

		case 'L':
		  return Convert.ToInt64(parseLong());

		case Hessian2Constants_Fields.BC_DOUBLE_ZERO:
		  return Convert.ToDouble(0);

		case Hessian2Constants_Fields.BC_DOUBLE_ONE:
		  return Convert.ToDouble(1);

		case Hessian2Constants_Fields.BC_DOUBLE_BYTE:
		  return Convert.ToDouble((sbyte) read());

		case Hessian2Constants_Fields.BC_DOUBLE_SHORT:
		  return Convert.ToDouble((short)(256 * read() + read()));

		case Hessian2Constants_Fields.BC_DOUBLE_MILL:
		{
		int mills = parseInt();

		return Convert.ToDouble(0.001 * mills);
		}

		case 'D':
		  return Convert.ToDouble(parseDouble());

		case Hessian2Constants_Fields.BC_DATE:
		  return new DateTime(parseLong());

		case Hessian2Constants_Fields.BC_DATE_MINUTE:
		  return new DateTime(parseInt() * 60000L);

		case Hessian2Constants_Fields.BC_STRING_CHUNK:
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

		case 0x00:
	case 0x01:
	case 0x02:
	case 0x03:
		case 0x04:
	case 0x05:
	case 0x06:
	case 0x07:
		case 0x08:
	case 0x09:
	case 0x0a:
	case 0x0b:
		case 0x0c:
	case 0x0d:
	case 0x0e:
	case 0x0f:

		case 0x10:
	case 0x11:
	case 0x12:
	case 0x13:
		case 0x14:
	case 0x15:
	case 0x16:
	case 0x17:
		case 0x18:
	case 0x19:
	case 0x1a:
	case 0x1b:
		case 0x1c:
	case 0x1d:
	case 0x1e:
	case 0x1f:
	{
		_isLastChunk = true;
		_chunkLength = tag - 0x00;

		int data;
		_sbuf.Length = 0;

		while ((data = parseChar()) >= 0)
		{
		  _sbuf.Append((char) data);
		}

		return _sbuf.ToString();
	}

		case 0x30:
	case 0x31:
	case 0x32:
	case 0x33:
	{
		_isLastChunk = true;
		_chunkLength = (tag - 0x30) * 256 + read();

		_sbuf.Length = 0;

		int ch;
		while ((ch = parseChar()) >= 0)
		{
		  _sbuf.Append((char) ch);
		}

		return _sbuf.ToString();
	}

		case Hessian2Constants_Fields.BC_BINARY_CHUNK:
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

		case 0x20:
	case 0x21:
	case 0x22:
	case 0x23:
		case 0x24:
	case 0x25:
	case 0x26:
	case 0x27:
		case 0x28:
	case 0x29:
	case 0x2a:
	case 0x2b:
		case 0x2c:
	case 0x2d:
	case 0x2e:
	case 0x2f:
	{
		_isLastChunk = true;
		int len = tag - 0x20;
		_chunkLength = 0;

		sbyte[] data = new sbyte[len];

		for (int i = 0; i < len; i++)
		{
		  data[i] = (sbyte) read();
		}

		return data;
	}

		case 0x34:
	case 0x35:
	case 0x36:
	case 0x37:
	{
		_isLastChunk = true;
		int len = (tag - 0x34) * 256 + read();
		_chunkLength = 0;

		sbyte[] buffer = new sbyte[len];

		for (int i = 0; i < len; i++)
		{
		  buffer[i] = (sbyte) read();
		}

		return buffer;
	}

		case Hessian2Constants_Fields.BC_LIST_VARIABLE:
		{
		// variable length list
		string type = readType();

		return findSerializerFactory().readList(this, -1, type);
		}

		case Hessian2Constants_Fields.BC_LIST_VARIABLE_UNTYPED:
		{
		return findSerializerFactory().readList(this, -1, null);
		}

		case Hessian2Constants_Fields.BC_LIST_FIXED:
		{
		// fixed length lists
		string type = readType();
		int length = readInt();

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(type, null);

		return reader.readLengthList(this, length);
		}

		case Hessian2Constants_Fields.BC_LIST_FIXED_UNTYPED:
		{
		// fixed length lists
		int length = readInt();

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(null, null);

		return reader.readLengthList(this, length);
		}

		  // compact fixed list
		case 0x70:
	case 0x71:
	case 0x72:
	case 0x73:
		case 0x74:
	case 0x75:
	case 0x76:
	case 0x77:
	{
		// fixed length lists
		string type = readType();
		int length = tag - 0x70;

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(type, null);

		return reader.readLengthList(this, length);
	}

		  // compact fixed untyped list
		case 0x78:
	case 0x79:
	case 0x7a:
	case 0x7b:
		case 0x7c:
	case 0x7d:
	case 0x7e:
	case 0x7f:
	{
		// fixed length lists
		int length = tag - 0x78;

		Deserializer reader;
		reader = findSerializerFactory().getListDeserializer(null, null);

		return reader.readLengthList(this, length);
	}

		case 'H':
		{
		return findSerializerFactory().readMap(this, null);
		}

		case 'M':
		{
		string type = readType();

		return findSerializerFactory().readMap(this, type);
		}

		case 'C':
		{
		readObjectDefinition(null);

		return readObject();
		}

		case 0x60:
	case 0x61:
	case 0x62:
	case 0x63:
		case 0x64:
	case 0x65:
	case 0x66:
	case 0x67:
		case 0x68:
	case 0x69:
	case 0x6a:
	case 0x6b:
		case 0x6c:
	case 0x6d:
	case 0x6e:
	case 0x6f:
	{
		int @ref = tag - 0x60;

		if (_classDefs == null)
		{
		  throw error("No classes defined at reference '{0}'" + tag);
		}

		ObjectDefinition def = (ObjectDefinition) _classDefs[@ref];

		return readObjectInstance(null, def);
	}

		case 'O':
		{
		int @ref = readInt();

		ObjectDefinition def = (ObjectDefinition) _classDefs[@ref];

		return readObjectInstance(null, def);
		}

		case Hessian2Constants_Fields.BC_REF:
		{
		int @ref = readInt();

		return _refs[@ref];
		}

		default:
		  if (tag < 0)
		  {
		throw new EOFException("readObject: unexpected end of file");
		  }
		  else
		  {
		throw error("readObject: unknown code " + codeName(tag));
		  }
		}
	  }

	  /// <summary>
	  /// Reads an object definition:
	  /// 
	  /// <pre>
	  /// O string <int> (string)* <value>*
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void readObjectDefinition(Class cl) throws IOException
	  private void readObjectDefinition(Type cl)
	  {
		string type = readString();
		int len = readInt();

		string[] fieldNames = new string[len];
		for (int i = 0; i < len; i++)
		{
		  fieldNames[i] = readString();
		}

		ObjectDefinition def = new ObjectDefinition(type, fieldNames);

		if (_classDefs == null)
		{
		  _classDefs = new ArrayList();
		}

		_classDefs.Add(def);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Object readObjectInstance(Class cl, ObjectDefinition def) throws IOException
	  private object readObjectInstance(Type cl, ObjectDefinition def)
	  {
		string type = def.Type;
		string[] fieldNames = def.FieldNames;

		if (cl != null)
		{
		  Deserializer reader;
		  reader = findSerializerFactory().getObjectDeserializer(type, cl);

		  return reader.readObject(this, fieldNames);
		}
		else
		{
		  return findSerializerFactory().readObject(this, type, fieldNames);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String readLenString() throws IOException
	  private string readLenString()
	  {
		int len = readInt();

		_isLastChunk = true;
		_chunkLength = len;

		_sbuf.Length = 0;
		int ch;
		while ((ch = parseChar()) >= 0)
		{
		  _sbuf.Append((char) ch);
		}

		return _sbuf.ToString();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String readLenString(int len) throws IOException
	  private string readLenString(int len)
	  {
		_isLastChunk = true;
		_chunkLength = len;

		_sbuf.Length = 0;
		int ch;
		while ((ch = parseChar()) >= 0)
		{
		  _sbuf.Append((char) ch);
		}

		return _sbuf.ToString();
	  }

	  /// <summary>
	  /// Reads a remote object.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readRemote() throws IOException
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
//ORIGINAL LINE: public Object readRef() throws IOException
	  public override object readRef()
	  {
		return _refs[parseInt()];
	  }

	  /// <summary>
	  /// Reads the start of a list.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readListStart() throws IOException
	  public override int readListStart()
	  {
		return read();
	  }

	  /// <summary>
	  /// Reads the start of a list.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readMapStart() throws IOException
	  public override int readMapStart()
	  {
		return read();
	  }

	  /// <summary>
	  /// Returns true if this is the end of a list or a map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isEnd() throws IOException
	  public override bool End
	  {
		  get
		  {
			int code;
    
			if (_offset < _length)
			{
			  code = (_buffer[_offset] & 0xff);
			}
			else
			{
			  code = read();
    
			  if (code >= 0)
			  {
			_offset--;
			  }
			}
    
			return (code < 0 || code == 'Z');
		  }
	  }

	  /// <summary>
	  /// Reads the end byte.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readEnd() throws IOException
	  public override void readEnd()
	  {
		int code = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		if (code == 'Z')
		{
		  return;
		}
		else if (code < 0)
		{
		  throw error("unexpected end of file");
		}
		else
		{
		  throw error("unknown code:" + codeName(code));
		}
	  }

	  /// <summary>
	  /// Reads the end byte.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readMapEnd() throws IOException
	  public override void readMapEnd()
	  {
		int code = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		if (code != 'Z')
		{
		  throw error("expected end of map ('Z') at '" + codeName(code) + "'");
		}
	  }

	  /// <summary>
	  /// Reads the end byte.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readListEnd() throws IOException
	  public override void readListEnd()
	  {
		int code = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		if (code != 'Z')
		{
		  throw error("expected end of list ('Z') at '" + codeName(code) + "'");
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readStreamingObject() throws IOException
	  public virtual object readStreamingObject()
	  {
		if (_refs != null)
		{
		  _refs.Clear();
		}

		return readObject();
	  }

	  /// <summary>
	  /// Resolves a remote object.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object resolveRemote(String type, String url) throws IOException
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
	  /// type ::= string
	  /// type ::= int
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readType() throws IOException
	  public override string readType()
	  {
		int code = _offset < _length ? (_buffer[_offset++] & 0xff) : read();
		_offset--;

		switch (code)
		{
		case 0x00:
	case 0x01:
	case 0x02:
	case 0x03:
		case 0x04:
	case 0x05:
	case 0x06:
	case 0x07:
		case 0x08:
	case 0x09:
	case 0x0a:
	case 0x0b:
		case 0x0c:
	case 0x0d:
	case 0x0e:
	case 0x0f:

		case 0x10:
	case 0x11:
	case 0x12:
	case 0x13:
		case 0x14:
	case 0x15:
	case 0x16:
	case 0x17:
		case 0x18:
	case 0x19:
	case 0x1a:
	case 0x1b:
		case 0x1c:
	case 0x1d:
	case 0x1e:
	case 0x1f:

		case 0x30:
	case 0x31:
	case 0x32:
	case 0x33:
		case Hessian2Constants_Fields.BC_STRING_CHUNK:
	case 'S':
	{
		string type = readString();

		if (_types == null)
		{
		  _types = new ArrayList();
		}

		_types.Add(type);

		return type;
	}

		default:
		{
		int @ref = readInt();

		if (_types.Count <= @ref)
		{
		  throw new System.IndexOutOfRangeException("type ref #" + @ref + " is greater than the number of valid types (" + _types.Count + ")");
		}

		return (string) _types[@ref];
		}
		}
	  }

	  /// <summary>
	  /// Parses the length for an array
	  /// 
	  /// <pre>
	  /// l b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readLength() throws IOException
	  public override int readLength()
	  {
		throw new System.NotSupportedException();
	  }

	  /// <summary>
	  /// Parses a 32-bit integer value from the stream.
	  /// 
	  /// <pre>
	  /// b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int parseInt() throws IOException
	  private int parseInt()
	  {
		int offset = _offset;

		if (offset + 3 < _length)
		{
		  sbyte[] buffer = _buffer;

		  int b32 = buffer[offset + 0] & 0xff;
		  int b24 = buffer[offset + 1] & 0xff;
		  int b16 = buffer[offset + 2] & 0xff;
		  int b8 = buffer[offset + 3] & 0xff;

		  _offset = offset + 4;

		  return (b32 << 24) + (b24 << 16) + (b16 << 8) + b8;
		}
		else
		{
		  int b32 = read();
		  int b24 = read();
		  int b16 = read();
		  int b8 = read();

		  return (b32 << 24) + (b24 << 16) + (b16 << 8) + b8;
		}
	  }

	  /// <summary>
	  /// Parses a 64-bit long value from the stream.
	  /// 
	  /// <pre>
	  /// b64 b56 b48 b40 b32 b24 b16 b8
	  /// </pre>
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private long parseLong() throws IOException
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
//ORIGINAL LINE: private double parseDouble() throws IOException
	  private double parseDouble()
	  {
		long bits = parseLong();

		return Double.longBitsToDouble(bits);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: org.w3c.dom.Node parseXML() throws IOException
	  internal virtual org.w3c.dom.Node parseXML()
	  {
		throw new System.NotSupportedException();
	  }

	  /// <summary>
	  /// Reads a character from the underlying stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int parseChar() throws IOException
	  private int parseChar()
	  {
		while (_chunkLength <= 0)
		{
		  if (_isLastChunk)
		  {
			return -1;
		  }

		  int code = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

		  switch (code)
		  {
		  case Hessian2Constants_Fields.BC_STRING_CHUNK:
			_isLastChunk = false;

			_chunkLength = (read() << 8) + read();
			break;

		  case 'S':
			_isLastChunk = true;

			_chunkLength = (read() << 8) + read();
			break;

		  case 0x00:
	  case 0x01:
	case 0x02:
	case 0x03:
		  case 0x04:
	  case 0x05:
	case 0x06:
	case 0x07:
		  case 0x08:
	  case 0x09:
	case 0x0a:
	case 0x0b:
		  case 0x0c:
	  case 0x0d:
	case 0x0e:
	case 0x0f:

		  case 0x10:
	  case 0x11:
	case 0x12:
	case 0x13:
		  case 0x14:
	  case 0x15:
	case 0x16:
	case 0x17:
		  case 0x18:
	  case 0x19:
	case 0x1a:
	case 0x1b:
		  case 0x1c:
	  case 0x1d:
	case 0x1e:
	case 0x1f:
		_isLastChunk = true;
		_chunkLength = code - 0x00;
		break;

		// qian.lei 2010-7-21
		  case 0x30:
	  case 0x31:
	case 0x32:
	case 0x33:
		_isLastChunk = true;
		_chunkLength = ((code - 0x30) << 8) + read();
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
//ORIGINAL LINE: private int parseUTF8Char() throws IOException
	  private int parseUTF8Char()
	  {
		int ch = _offset < _length ? (_buffer[_offset++] & 0xff) : read();

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
//ORIGINAL LINE: private int parseByte() throws IOException
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
		  case Hessian2Constants_Fields.BC_BINARY_CHUNK:
			_isLastChunk = false;

			_chunkLength = (read() << 8) + read();
			break;

		  case 'B':
			_isLastChunk = true;

			_chunkLength = (read() << 8) + read();
			break;

		  case 0x20:
	  case 0x21:
	case 0x22:
	case 0x23:
		  case 0x24:
	  case 0x25:
	case 0x26:
	case 0x27:
		  case 0x28:
	  case 0x29:
	case 0x2a:
	case 0x2b:
		  case 0x2c:
	  case 0x2d:
	case 0x2e:
	case 0x2f:
			_isLastChunk = true;

			_chunkLength = code - 0x20;
			break;

		  case 0x34:
	  case 0x35:
	case 0x36:
	case 0x37:
		_isLastChunk = true;
			_chunkLength = (code - 0x34) * 256 + read();
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
//ORIGINAL LINE: public InputStream readInputStream() throws IOException
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

		case 0x20:
	case 0x21:
	case 0x22:
	case 0x23:
		case 0x24:
	case 0x25:
	case 0x26:
	case 0x27:
		case 0x28:
	case 0x29:
	case 0x2a:
	case 0x2b:
		case 0x2c:
	case 0x2d:
	case 0x2e:
	case 0x2f:
		  _isLastChunk = true;
		  _chunkLength = tag - 0x20;
		  break;

		default:
		  throw expect("binary", tag);
		}

		return new ReadInputStream(this);
	  }

	  /// <summary>
	  /// Reads bytes from the underlying stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int read(byte [] buffer, int offset, int length) throws IOException
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

		case 0x20:
	case 0x21:
	case 0x22:
	case 0x23:
		case 0x24:
	case 0x25:
	case 0x26:
	case 0x27:
		case 0x28:
	case 0x29:
	case 0x2a:
	case 0x2b:
		case 0x2c:
	case 0x2d:
	case 0x2e:
	case 0x2f:
		  _isLastChunk = true;
		  _chunkLength = code - 0x20;
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

		  if (_length <= _offset && !readBuffer())
		  {
		return -1;
		  }

		  if (_length - _offset < sublen)
		  {
		sublen = _length - _offset;
		  }

		  Array.Copy(_buffer, _offset, buffer, offset, sublen);

		  _offset += sublen;

		  offset += sublen;
		  readLength += sublen;
		  length -= sublen;
		  _chunkLength -= sublen;
		}

		return readLength;
	  }

	  /// <summary>
	  /// Normally, shouldn't be called externally, but needed for QA, e.g.
	  /// ejb/3b01.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final int read() throws IOException
	  public int read()
	  {
		if (_length <= _offset && !readBuffer())
		{
		  return -1;
		}

		return _buffer[_offset++] & 0xff;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private final boolean readBuffer() throws IOException
	  private bool readBuffer()
	  {
		sbyte[] buffer = _buffer;
		int offset = _offset;
		int length = _length;

		if (offset < length)
		{
		  Array.Copy(buffer, offset, buffer, 0, length - offset);
		  offset = length - offset;
		}
		else
		{
		  offset = 0;
		}

		int len = _is.Read(buffer, offset, SIZE - offset);

		if (len <= 0)
		{
		  _length = offset;
		  _offset = 0;

		  return offset > 0;
		}

		_length = offset + len;
		_offset = 0;

		return true;
	  }

	  public override Reader Reader
	  {
		  get
		  {
			return null;
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected IOException expect(String expect, int ch) throws IOException
	  protected internal virtual IOException expect(string expect, int ch)
	  {
		if (ch < 0)
		{
		  return error("expected " + expect + " at end of file");
		}
		else
		{
		  _offset--;

		  try
		  {
		object obj = readObject();

		if (obj != null)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  return error("expected " + expect + " at 0x" + (ch & 0xff).ToString("x") + " " + obj.GetType().FullName + " (" + obj + ")");
		}
		else
		{
		  return error("expected " + expect + " at 0x" + (ch & 0xff).ToString("x") + " null");
		}
		  }
		  catch (IOException e)
		  {
		log.log(Level.FINE, e.ToString(), e);

		return error("expected " + expect + " at 0x" + (ch & 0xff).ToString("x"));
		  }
		}
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws IOException
	  public override void close()
	  {
		System.IO.Stream @is = _is;
		_is = null;

		if (_isCloseStreamOnClose && @is != null)
		{
		  @is.Close();
		}
	  }

	  internal class ReadInputStream : System.IO.Stream
	  {
		  private readonly Hessian2Input outerInstance;

		  public ReadInputStream(Hessian2Input outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		internal bool _isClosed = false;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read() throws IOException
		public virtual int read()
		{
		  if (_isClosed)
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
//ORIGINAL LINE: public int read(byte [] buffer, int offset, int length) throws IOException
		public virtual int read(sbyte[] buffer, int offset, int length)
		{
		  if (_isClosed)
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
//ORIGINAL LINE: public void close() throws IOException
		public virtual void close()
		{
		  while (read() >= 0)
		  {
		  }
		}
	  }

	  internal sealed class ObjectDefinition
	  {
		internal readonly string _type;
		internal readonly string[] _fields;

		internal ObjectDefinition(string type, string[] fields)
		{
		  _type = type;
		  _fields = fields;
		}

		internal string Type
		{
			get
			{
			  return _type;
			}
		}

		internal string [] FieldNames
		{
			get
			{
			  return _fields;
			}
		}
	  }

	  static Hessian2Input()
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