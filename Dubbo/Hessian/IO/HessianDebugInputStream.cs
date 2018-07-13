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
	/// Debugging input stream for Hessian requests.
	/// </summary>
	public class HessianDebugInputStream : System.IO.Stream
	{
	  private System.IO.Stream _is;

	  private HessianDebugState _state;

	  /// <summary>
	  /// Creates an uninitialized Hessian input stream.
	  /// </summary>
	  public HessianDebugInputStream(System.IO.Stream @is, PrintWriter dbg)
	  {
		_is = @is;

		if (dbg == null)
		{
		  dbg = new PrintWriter(System.out);
		}

		_state = new HessianDebugState(dbg);
	  }

	  /// <summary>
	  /// Creates an uninitialized Hessian input stream.
	  /// </summary>
	  public HessianDebugInputStream(System.IO.Stream @is, Logger log, Level level) : this(@is, new PrintWriter(new LogWriter(log, level)))
	  {
	  }

	  public virtual void startTop2()
	  {
		_state.startTop2();
	  }

	  /// <summary>
	  /// Reads a character.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read() throws java.io.IOException
	  public virtual int read()
	  {
		int ch;

		System.IO.Stream @is = _is;

		if (@is == null)
		{
		  return -1;
		}
		else
		{
		  ch = @is.Read();
		}

		_state.next(ch);

		return ch;
	  }

	  /// <summary>
	  /// closes the stream.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public virtual void close()
	  {
		System.IO.Stream @is = _is;
		_is = null;

		if (@is != null)
		{
		  @is.Close();
		}

		_state.println();
	  }

	  internal class LogWriter : Writer
	  {
		internal Logger _log;
		internal Level _level;
		internal StringBuilder _sb = new StringBuilder();

		internal LogWriter(Logger log, Level level)
		{
		  _log = log;
		  _level = level;
		}

		public virtual void write(char ch)
		{
		  if (ch == '\n' && _sb.Length > 0)
		  {
		_log.log(_level, _sb.ToString());
		_sb.Length = 0;
		  }
		  else
		  {
		_sb.Append((char) ch);
		  }
		}

		public virtual void write(char[] buffer, int offset, int length)
		{
		  for (int i = 0; i < length; i++)
		  {
		char ch = buffer[offset + i];

		if (ch == '\n' && _sb.Length > 0)
		{
		  _log.log(_level, _sb.ToString());
		  _sb.Length = 0;
		}
		else
		{
		  _sb.Append((char) ch);
		}
		  }
		}

		public virtual void flush()
		{
		}

		public virtual void close()
		{
		}
	  }
	}

}