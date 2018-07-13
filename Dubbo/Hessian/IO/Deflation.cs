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


	using Hessian.IO;

	public class Deflation : HessianEnvelope
	{
	  public Deflation()
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Hessian2Output wrap(Hessian2Output out) throws IOException
	  public override Hessian2Output wrap(Hessian2Output @out)
	  {
		System.IO.Stream os = new DeflateOutputStream(@out);

		Hessian2Output filterOut = new Hessian2Output(os);

		filterOut.CloseStreamOnClose = true;

		return filterOut;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Hessian2Input unwrap(Hessian2Input in) throws IOException
	  public override Hessian2Input unwrap(Hessian2Input @in)
	  {
		int version = @in.readEnvelope();

		string method = @in.readMethod();

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		if (!method.Equals(this.GetType().FullName))
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new IOException("expected hessian Envelope method '" + this.GetType().FullName + "' at '" + method + "'");
		}

		return unwrapHeaders(@in);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Hessian2Input unwrapHeaders(Hessian2Input in) throws IOException
	  public override Hessian2Input unwrapHeaders(Hessian2Input @in)
	  {
		System.IO.Stream @is = new DeflateInputStream(@in);

		Hessian2Input filter = new Hessian2Input(@is);

		filter.CloseStreamOnClose = true;

		return filter;
	  }

	  internal class DeflateOutputStream : System.IO.Stream
	  {
		internal Hessian2Output _out;
		internal System.IO.Stream _bodyOut;
		internal DeflaterOutputStream _deflateOut;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: DeflateOutputStream(Hessian2Output out) throws IOException
		internal DeflateOutputStream(Hessian2Output @out)
		{
		  _out = @out;

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  _out.startEnvelope(typeof(Deflation).FullName);

		  _out.writeInt(0);

		  _bodyOut = _out.getBytesOutputStream();

		  _deflateOut = new DeflaterOutputStream(_bodyOut);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(int ch) throws IOException
		public virtual void write(int ch)
		{
		  _deflateOut.write(ch);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(byte [] buffer, int offset, int length) throws IOException
		public virtual void write(sbyte[] buffer, int offset, int length)
		{
		  _deflateOut.write(buffer, offset, length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws IOException
		public virtual void close()
		{
		  Hessian2Output @out = _out;
		  _out = null;

		  if (@out != null)
		  {
		_deflateOut.close();
		_bodyOut.Close();

		@out.writeInt(0);

			@out.completeEnvelope();

		@out.close();
		  }
		}
	  }

	  internal class DeflateInputStream : System.IO.Stream
	  {
		internal Hessian2Input _in;

		internal System.IO.Stream _bodyIn;
		internal InflaterInputStream _inflateIn;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: DeflateInputStream(Hessian2Input in) throws IOException
		internal DeflateInputStream(Hessian2Input @in)
		{
		  _in = @in;

		  int len = @in.readInt();

		  if (len != 0)
		  {
			throw new IOException("expected no headers");
		  }

		  _bodyIn = _in.readInputStream();

		  _inflateIn = new InflaterInputStream(_bodyIn);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read() throws IOException
		public virtual int read()
		{
		  return _inflateIn.read();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read(byte [] buffer, int offset, int length) throws IOException
		public virtual int read(sbyte[] buffer, int offset, int length)
		{
		  return _inflateIn.read(buffer, offset, length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws IOException
		public virtual void close()
		{
		  Hessian2Input @in = _in;
		  _in = null;

		  if (@in != null)
		  {
		_inflateIn.close();
		_bodyIn.Close();

		int len = @in.readInt();

		if (len != 0)
		{
		  throw new IOException("Unexpected footer");
		}

			@in.completeEnvelope();

		@in.close();
		  }
		}
	  }
	}

}