using System;
using System.Collections.Generic;
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
	/// Debugging input stream for Hessian requests.
	/// </summary>
	public class HessianDebugState : Hessian2Constants
	{
	  private PrintWriter _dbg;

	  private State _state;
	  private List<State> _stateStack = new List<State>();

	  private List<ObjectDef> _objectDefList = new List<ObjectDef>();

	  private List<string> _typeDefList = new List<string>();

	  private int _refId;
	  private bool _isNewline = true;
	  private bool _isObject = false;
	  private int _column;

	  /// <summary>
	  /// Creates an uninitialized Hessian input stream.
	  /// </summary>
	  public HessianDebugState(PrintWriter dbg)
	  {
		_dbg = dbg;

		_state = new InitialState(this);
	  }

	  public virtual void startTop2()
	  {
		_state = new Top2State(this);
	  }

	  /// <summary>
	  /// Reads a character.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void next(int ch) throws java.io.IOException
	  public virtual void next(int ch)
	  {
		_state = _state.next(ch);
	  }

	  internal virtual void pushStack(State state)
	  {
		_stateStack.Add(state);
	  }

	  internal virtual State popStack()
	  {
		return _stateStack.Remove(_stateStack.Count - 1);
	  }

	  internal virtual void println()
	  {
		if (!_isNewline)
		{
		  _dbg.println();
		  _dbg.flush();
		}

		_isNewline = true;
		_column = 0;
	  }

	  internal static bool isString(int ch)
	  {
		switch (ch)
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

		case 'R':
		case 'S':
		  return true;

		default:
		  return false;
		}
	  }

	  internal static bool isInteger(int ch)
	  {
		switch (ch)
		{
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

		case 0xd0:
	case 0xd1:
	case 0xd2:
	case 0xd3:
		case 0xd4:
	case 0xd5:
	case 0xd6:
	case 0xd7:

		case 'I':
		  return true;

		default:
		  return false;
		}
	  }

	  internal abstract class State
	  {
		  private readonly HessianDebugState outerInstance;

		internal State _next;

		internal State(HessianDebugState outerInstance)
		{
			this.outerInstance = outerInstance;
		}

		internal State(HessianDebugState outerInstance, State next)
		{
			this.outerInstance = outerInstance;
		  _next = next;
		}

		internal abstract State next(int ch);

		internal virtual bool isShift(object value)
		{
		  return false;
		}

		internal virtual State shift(object value)
		{
		  return this;
		}

		internal virtual int depth()
		{
		  if (_next != null)
		  {
		return _next.depth();
		  }
		  else
		  {
		return 0;
		  }
		}

		internal virtual void printIndent(int depth)
		{
		  if (outerInstance._isNewline)
		  {
		for (int i = outerInstance._column; i < depth() + depth; i++)
		{
		  outerInstance._dbg.print(" ");
		  outerInstance._column++;
		}
		  }
		}

		internal virtual void print(string @string)
		{
		  print(0, @string);
		}

		internal virtual void print(int depth, string @string)
		{
		  printIndent(depth);

		  outerInstance._dbg.print(@string);
		  outerInstance._isNewline = false;
		  outerInstance._isObject = false;

		  int p = @string.LastIndexOf('\n');
		  if (p > 0)
		  {
		outerInstance._column = @string.Length - p - 1;
		  }
		  else
		  {
		outerInstance._column += @string.Length;
		  }
		}

		internal virtual void println(string @string)
		{
		  println(0, @string);
		}

		internal virtual void println(int depth, string @string)
		{
		  printIndent(depth);

		  outerInstance._dbg.println(@string);
		  outerInstance._dbg.flush();
		  outerInstance._isNewline = true;
		  outerInstance._isObject = false;
		  outerInstance._column = 0;
		}

		internal virtual void println()
		{
		  if (!outerInstance._isNewline)
		  {
		outerInstance._dbg.println();
		outerInstance._dbg.flush();
		  }

		  outerInstance._isNewline = true;
		  outerInstance._isObject = false;
		  outerInstance._column = 0;
		}

		internal virtual void printObject(string @string)
		{
		  if (outerInstance._isObject)
		  {
		println();
		  }

		  printIndent(0);

		  outerInstance._dbg.print(@string);
		  outerInstance._dbg.flush();

		  outerInstance._column += @string.Length;

		  outerInstance._isNewline = false;
		  outerInstance._isObject = true;
		}

		protected internal virtual State nextObject(int ch)
		{
		  switch (ch)
		  {
		  case -1:
		println();
		return this;

		  case 'N':
		if (isShift(null))
		{
		  return shift(null);
		}
		else
		{
		  printObject("null");
		  return this;
		}

		  case 'T':
		if (isShift(true))
		{
		  return shift(true);
		}
		else
		{
		  printObject("true");
		  return this;
		}

		  case 'F':
		if (isShift(false))
		{
		  return shift(false);
		}
		else
		{
		  printObject("false");
		  return this;
		}

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
	{
		  int? value = new int?(ch - 0x90);

		  if (isShift(value))
		  {
			return shift(value);
		  }
		  else
		  {
			printObject(value.ToString());
			return this;
		  }
	}

			  goto case 0xc0;
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
		return new IntegerState(outerInstance, this, "int", ch - 0xc8, 3);

		  case 0xd0:
	  case 0xd1:
	case 0xd2:
	case 0xd3:
		  case 0xd4:
	  case 0xd5:
	case 0xd6:
	case 0xd7:
		return new IntegerState(outerInstance, this, "int", ch - 0xd4, 2);

		  case 'I':
		return new IntegerState(outerInstance, this, "int");

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
	{
		  long? value = new long?(ch - 0xe0);

		  if (isShift(value))
		  {
			return shift(value);
		  }
		  else
		  {
			printObject(value.ToString() + "L");
			return this;
		  }
	}

			  goto case 0xf0;
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
		return new LongState(outerInstance, this, "long", ch - 0xf8, 7);

		  case 0x38:
	  case 0x39:
	case 0x3a:
	case 0x3b:
		  case 0x3c:
	  case 0x3d:
	case 0x3e:
	case 0x3f:
		return new LongState(outerInstance, this, "long", ch - 0x3c, 6);

		  case Hessian2Constants_Fields.BC_LONG_INT:
		return new LongState(outerInstance, this, "long", 0, 4);

		  case 'L':
		return new LongState(outerInstance, this, "long");

		  case 0x5b:
	  case 0x5c:
	  {
		  double? value = new double?(ch - 0x5b);

		  if (isShift(value))
		  {
			return shift(value);
		  }
		  else
		  {
			printObject(value.ToString());
			return this;
		  }
	  }

			  goto case 0x5d;
		  case 0x5d:
		return new DoubleIntegerState(outerInstance, this, 3);

		  case 0x5e:
		return new DoubleIntegerState(outerInstance, this, 2);

		  case 0x5f:
		return new MillsState(outerInstance, this);

		  case 'D':
		return new DoubleState(outerInstance, this);

		  case 'Q':
		return new RefState(outerInstance, this);

		  case Hessian2Constants_Fields.BC_DATE:
		return new DateState(outerInstance, this);

		  case Hessian2Constants_Fields.BC_DATE_MINUTE:
		return new DateState(outerInstance, this, true);

		  case 0x00:
		  {
		  string value = "\"\"";

		  if (isShift(value))
		  {
			return shift(value);
		  }
		  else
		  {
			printObject(value.ToString());
			return this;
		  }
		  }

			  goto case 0x01;
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
		return new StringState(outerInstance, this, 'S', ch);

		  case 0x30:
	  case 0x31:
	case 0x32:
	case 0x33:
		return new StringState(outerInstance, this, 'S', ch - 0x30, true);

		  case 'R':
		return new StringState(outerInstance, this, 'S', false);

		  case 'S':
		return new StringState(outerInstance, this, 'S', true);

		  case 0x20:
		  {
		  string value = "binary(0)";

		  if (isShift(value))
		  {
			return shift(value);
		  }
		  else
		  {
			printObject(value.ToString());
			return this;
		  }
		  }

			  goto case 0x21;
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
		return new BinaryState(outerInstance, this, 'B', ch - 0x20);

		  case 0x34:
	  case 0x35:
	case 0x36:
	case 0x37:
		return new BinaryState(outerInstance, this, 'B', ch - 0x34, true);

		  case 'A':
		return new BinaryState(outerInstance, this, 'B', false);

		  case 'B':
		return new BinaryState(outerInstance, this, 'B', true);

		  case 'M':
		return new MapState(outerInstance, this, outerInstance._refId++);

		  case 'H':
		return new MapState(outerInstance, this, outerInstance._refId++, false);

		  case Hessian2Constants_Fields.BC_LIST_VARIABLE:
		return new ListState(outerInstance, this, outerInstance._refId++, true);

		  case Hessian2Constants_Fields.BC_LIST_VARIABLE_UNTYPED:
		return new ListState(outerInstance, this, outerInstance._refId++, false);

		  case Hessian2Constants_Fields.BC_LIST_FIXED:
		return new CompactListState(outerInstance, this, outerInstance._refId++, true);

		  case Hessian2Constants_Fields.BC_LIST_FIXED_UNTYPED:
		return new CompactListState(outerInstance, this, outerInstance._refId++, false);

		  case 0x70:
	  case 0x71:
	case 0x72:
	case 0x73:
		  case 0x74:
	  case 0x75:
	case 0x76:
	case 0x77:
		return new CompactListState(outerInstance, this, outerInstance._refId++, true, ch - 0x70);

		  case 0x78:
	  case 0x79:
	case 0x7a:
	case 0x7b:
		  case 0x7c:
	  case 0x7d:
	case 0x7e:
	case 0x7f:
		return new CompactListState(outerInstance, this, outerInstance._refId++, false, ch - 0x78);

		  case 'C':
		return new ObjectDefState(outerInstance, this);

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
		return new ObjectState(outerInstance, this, outerInstance._refId++, ch - 0x60);

		  case 'O':
		return new ObjectState(outerInstance, this, outerInstance._refId++);

		  default:
		return this;
		  }
		}
	  }

	  internal class InitialState : State
	  {
		  private readonly HessianDebugState outerInstance;

		  public InitialState(HessianDebugState outerInstance) : base(outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		internal override State next(int ch)
		{
		  println();

		  if (ch == 'r')
		  {
		return new ReplyState(outerInstance, this);
		  }
		  else if (ch == 'c')
		  {
		return new CallState(outerInstance, this);
		  }
		  else
		  {
		return nextObject(ch);
		  }
		}
	  }

	  internal class Top2State : State
	  {
		  private readonly HessianDebugState outerInstance;

		  public Top2State(HessianDebugState outerInstance) : base(outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		internal override State next(int ch)
		{
		  println();

		  if (ch == 'R')
		  {
		return new Reply2State(outerInstance, this);
		  }
		  else if (ch == 'F')
		  {
		return new Fault2State(outerInstance, this);
		  }
		  else if (ch == 'C')
		  {
		return new Call2State(outerInstance, this);
		  }
		  else if (ch == 'H')
		  {
		return new Hessian2State(outerInstance, this);
		  }
		  else if (ch == 'r')
		  {
		return new ReplyState(outerInstance, this);
		  }
		  else if (ch == 'c')
		  {
		return new CallState(outerInstance, this);
		  }
		  else
		  {
		return nextObject(ch);
		  }
		}
	  }

	  internal class IntegerState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal string _typeCode;

		internal int _length;
		internal int _value;

		internal IntegerState(HessianDebugState outerInstance, State next, string typeCode) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		}

		internal IntegerState(HessianDebugState outerInstance, State next, string typeCode, int value, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;

		  _value = value;
		  _length = length;
		}

		internal override State next(int ch)
		{
		  _value = 256 * _value + (ch & 0xff);

		  if (++_length == 4)
		  {
		int? value = new int?(_value);

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value.ToString());

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class LongState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal string _typeCode;

		internal int _length;
		internal long _value;

		internal LongState(HessianDebugState outerInstance, State next, string typeCode) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		}

		internal LongState(HessianDebugState outerInstance, State next, string typeCode, long value, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;

		  _value = value;
		  _length = length;
		}

		internal override State next(int ch)
		{
		  _value = 256 * _value + (ch & 0xff);

		  if (++_length == 8)
		  {
		long? value = new long?(_value);

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value.ToString() + "L");

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class DoubleIntegerState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal int _length;
		internal int _value;
		internal bool _isFirst = true;

		internal DoubleIntegerState(HessianDebugState outerInstance, State next, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _length = length;
		}

		internal override State next(int ch)
		{
		  if (_isFirst)
		  {
		_value = (sbyte) ch;
		  }
		  else
		  {
		_value = 256 * _value + (ch & 0xff);
		  }

		  _isFirst = false;

		  if (++_length == 4)
		  {
		double? value = new double?(_value);

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value.ToString());

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class RefState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal string _typeCode;

		internal int _length;
		internal int _value;

		internal RefState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal RefState(HessianDebugState outerInstance, State next, string typeCode) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		}

		internal RefState(HessianDebugState outerInstance, State next, string typeCode, int value, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;

		  _value = value;
		  _length = length;
		}

		internal override bool isShift(object o)
		{
		  return true;
		}

		internal override State shift(object o)
		{
		  println("ref #" + o);

		  return _next;
		}

		internal override State next(int ch)
		{
		  return nextObject(ch);
		}
	  }

	  internal class DateState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal int _length;
		internal long _value;
		internal bool _isMinute;

		internal DateState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal DateState(HessianDebugState outerInstance, State next, bool isMinute) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _length = 4;
		  _isMinute = isMinute;
		}


		internal override State next(int ch)
		{
		  _value = 256 * _value + (ch & 0xff);

		  if (++_length == 8)
		  {
		DateTime value;

		if (_isMinute)
		{
		  value = new DateTime(_value * 60000L);
		}
		else
		{
		  value = new DateTime(_value);
		}

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value.ToString());

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class DoubleState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal int _length;
		internal long _value;

		internal DoubleState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override State next(int ch)
		{
		  _value = 256 * _value + (ch & 0xff);

		  if (++_length == 8)
		  {
		double? value = Double.longBitsToDouble(_value);

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value.ToString());

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class MillsState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal int _length;
		internal int _value;

		internal MillsState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override State next(int ch)
		{
		  _value = 256 * _value + (ch & 0xff);

		  if (++_length == 4)
		  {
		double? value = 0.001 * _value;

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value.ToString());

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class StringState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TOP = 0;
		internal const int UTF_2_1 = 1;
		internal const int UTF_3_1 = 2;
		internal const int UTF_3_2 = 3;

		internal char _typeCode;

		internal StringBuilder _value = new StringBuilder();
		internal int _lengthIndex;
		internal int _length;
		internal bool _isLastChunk;

		internal int _utfState;
		internal char _ch;

		internal StringState(HessianDebugState outerInstance, State next, char typeCode, bool isLastChunk) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		  _isLastChunk = isLastChunk;
		}

		internal StringState(HessianDebugState outerInstance, State next, char typeCode, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		  _isLastChunk = true;
		  _length = length;
		  _lengthIndex = 2;
		}

		internal StringState(HessianDebugState outerInstance, State next, char typeCode, int length, bool isLastChunk) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		  _isLastChunk = isLastChunk;
		  _length = length;
		  _lengthIndex = 1;
		}

		internal override State next(int ch)
		{
		  if (_lengthIndex < 2)
		  {
		_length = 256 * _length + (ch & 0xff);

		if (++_lengthIndex == 2 && _length == 0 && _isLastChunk)
		{
		  if (_next.isShift(_value.ToString()))
		  {
			return _next.shift(_value.ToString());
		  }
		  else
		  {
			printObject("\"" + _value + "\"");
			return _next;
		  }
		}
		else
		{
		  return this;
		}
		  }
		  else if (_length == 0)
		  {
		if (ch == 's' || ch == 'x')
		{
		  _isLastChunk = false;
		  _lengthIndex = 0;
		  return this;
		}
		else if (ch == 'S' || ch == 'X')
		{
		  _isLastChunk = true;
		  _lengthIndex = 0;
		  return this;
		}
		else if (ch == 0x00)
		{
		  if (_next.isShift(_value.ToString()))
		  {
			return _next.shift(_value.ToString());
		  }
		  else
		  {
			printObject("\"" + _value + "\"");
			return _next;
		  }
		}
		else if (0x00 <= ch && ch < 0x20)
		{
		  _isLastChunk = true;
		  _lengthIndex = 2;
		  _length = ch & 0xff;
		  return this;
		}
		else if (0x30 <= ch && ch < 0x34)
		{
		  _isLastChunk = true;
		  _lengthIndex = 1;
		  _length = (ch - 0x30);
		  return this;
		}
		else
		{
		  println(((char) ch).ToString() + ": unexpected character");
		  return _next;
		}
		  }

		  switch (_utfState)
		  {
		  case TOP:
		if (ch < 0x80)
		{
		  _length--;

		  _value.Append((char) ch);
		}
		else if (ch < 0xe0)
		{
		  _ch = (char)((ch & 0x1f) << 6);
		  _utfState = UTF_2_1;
		}
		else
		{
		  _ch = (char)((ch & 0xf) << 12);
		  _utfState = UTF_3_1;
		}
		break;

		  case UTF_2_1:
		  case UTF_3_2:
		_ch += ch & 0x3f;
		_value.Append(_ch);
		_length--;
		_utfState = TOP;
		break;

		  case UTF_3_1:
		_ch += (char)((ch & 0x3f) << 6);
		_utfState = UTF_3_2;
		break;
		  }

		  if (_length == 0 && _isLastChunk)
		  {
		if (_next.isShift(_value.ToString()))
		{
		  return _next.shift(_value.ToString());
		}
		else
		{
		  printObject("\"" + _value + "\"");

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class BinaryState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal char _typeCode;

		internal int _totalLength;

		internal int _lengthIndex;
		internal int _length;
		internal bool _isLastChunk;

		internal BinaryState(HessianDebugState outerInstance, State next, char typeCode, bool isLastChunk) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		  _isLastChunk = isLastChunk;
		}

		internal BinaryState(HessianDebugState outerInstance, State next, char typeCode, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		  _isLastChunk = true;
		  _length = length;
		  _lengthIndex = 2;
		}

		internal BinaryState(HessianDebugState outerInstance, State next, char typeCode, int length, bool isLastChunk) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _typeCode = typeCode;
		  _isLastChunk = isLastChunk;
		  _length = length;
		  _lengthIndex = 1;
		}

		internal override State next(int ch)
		{
		  if (_lengthIndex < 2)
		  {
		_length = 256 * _length + (ch & 0xff);

		if (++_lengthIndex == 2 && _length == 0 && _isLastChunk)
		{
		  string value = "binary(" + _totalLength + ")";

		  if (_next.isShift(value))
		  {
			return _next.shift(value);
		  }
		  else
		  {
			printObject(value);
			return _next;
		  }
		}
		else
		{
		  return this;
		}
		  }
		  else if (_length == 0)
		  {
		if (ch == 'b')
		{
		  _isLastChunk = false;
		  _lengthIndex = 0;
		  return this;
		}
		else if (ch == 'B')
		{
		  _isLastChunk = true;
		  _lengthIndex = 0;
		  return this;
		}
		else if (ch == 0x20)
		{
		  string value = "binary(" + _totalLength + ")";

		  if (_next.isShift(value))
		  {
			return _next.shift(value);
		  }
		  else
		  {
			printObject(value);
			return _next;
		  }
		}
		else if (0x20 <= ch && ch < 0x30)
		{
		  _isLastChunk = true;
		  _lengthIndex = 2;
		  _length = (ch & 0xff) - 0x20;
		  return this;
		}
		else
		{
		  println(((char) ch).ToString() + ": unexpected character");
		  return _next;
		}
		  }

		  _length--;
		  _totalLength++;

		  if (_length == 0 && _isLastChunk)
		  {
		string value = "binary(" + _totalLength + ")";

		if (_next.isShift(value))
		{
		  return _next.shift(value);
		}
		else
		{
		  printObject(value);

		  return _next;
		}
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class MapState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TYPE = 0;
		internal const int KEY = 1;
		internal const int VALUE = 2;

		internal int _refId;

		internal int _state;
		internal int _valueDepth;
		internal bool _hasData;

		internal MapState(HessianDebugState outerInstance, State next, int refId) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _refId = refId;
		  _state = TYPE;
		}

		internal MapState(HessianDebugState outerInstance, State next, int refId, bool isType) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _refId = refId;

		  if (isType)
		  {
		_state = TYPE;
		  }
		  else
		  {
		printObject("map (#" + _refId + ")");
		_state = VALUE;
		  }
		}

		internal override bool isShift(object value)
		{
		  return _state == TYPE;
		}

		internal override State shift(object type)
		{
		  if (_state == TYPE)
		  {
		if (type is string)
		{
		  outerInstance._typeDefList.Add((string) type);
		}
		else if (type is int?)
		{
		  int iValue = (int?) type.Value;

		  if (iValue >= 0 && iValue < outerInstance._typeDefList.Count)
		  {
			type = outerInstance._typeDefList[iValue];
		  }
		}

		printObject("map " + type + " (#" + _refId + ")");

		_state = VALUE;

		return this;
		  }
		  else
		  {
		throw new System.InvalidOperationException();
		  }
		}

		internal override int depth()
		{
		  if (_state == TYPE)
		  {
		return _next.depth();
		  }
		  else if (_state == KEY)
		  {
		return _next.depth() + 2;
		  }
		  else
		  {
		return _valueDepth;
		  }
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case TYPE:
		return nextObject(ch);

		  case VALUE:
		if (ch == 'Z')
		{
		  if (_hasData)
		  {
			println();
		  }

		  return _next;
		}
		else
		{
		  if (_hasData)
		  {
			println();
		  }

		  _hasData = true;
		  _state = KEY;

		  return nextObject(ch);
		}

		  case KEY:
		print(" => ");
		outerInstance._isObject = false;
		_valueDepth = outerInstance._column;

		_state = VALUE;

		return nextObject(ch);

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class ObjectDefState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TYPE = 1;
		internal const int COUNT = 2;
		internal const int FIELD = 3;
		internal const int COMPLETE = 4;

		internal int _refId;

		internal int _state;
		internal bool _hasData;
		internal int _count;

		internal string _type;
		internal List<string> _fields = new List<string>();

		internal ObjectDefState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _state = TYPE;
		}

		internal override bool isShift(object value)
		{
		  return true;
		}

		internal override State shift(object @object)
		{
		  if (_state == TYPE)
		  {
		_type = (string) @object;

		print("/* defun " + _type + " [");

		outerInstance._objectDefList.Add(new ObjectDef(_type, _fields));

		_state = COUNT;
		  }
		  else if (_state == COUNT)
		  {
		_count = (int?) @object;

		_state = FIELD;
		  }
		  else if (_state == FIELD)
		  {
		string field = (string) @object;

		_count--;

		_fields.Add(field);

		if (_fields.Count == 1)
		{
		  print(field);
		}
		else
		{
		  print(", " + field);
		}
		  }
		  else
		  {
		throw new System.NotSupportedException();
		  }

		  return this;
		}

		internal override int depth()
		{
		  if (_state <= TYPE)
		  {
		return _next.depth();
		  }
		  else
		  {
		return _next.depth() + 2;
		  }
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case TYPE:
		return nextObject(ch);

		  case COUNT:
		return nextObject(ch);

		  case FIELD:
		if (_count == 0)
		{
		  println("] */");
		  _next.printIndent(0);

		  return _next.nextObject(ch);
		}
		else
		{
		  return nextObject(ch);
		}

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class ObjectState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TYPE = 0;
		internal const int FIELD = 1;

		internal int _refId;

		internal int _state;
		internal ObjectDef _def;
		internal int _count;
		internal int _fieldDepth;

		internal ObjectState(HessianDebugState outerInstance, State next, int refId) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _refId = refId;
		  _state = TYPE;
		}

		internal ObjectState(HessianDebugState outerInstance, State next, int refId, int def) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _refId = refId;
		  _state = FIELD;

		  if (def < 0 || outerInstance._objectDefList.Count <= def)
		  {
		throw new System.InvalidOperationException(def + " is an unknown object type");
		  }

		  _def = outerInstance._objectDefList[def];

		  println("object " + _def.Type + " (#" + _refId + ")");
		}

		internal override bool isShift(object value)
		{
		  if (_state == TYPE)
		  {
		return true;
		  }
		  else
		  {
		return false;
		  }
		}

		internal override State shift(object @object)
		{
		  if (_state == TYPE)
		  {
		int def = (int?) @object;

		_def = outerInstance._objectDefList[def];

		println("object " + _def.Type + " (#" + _refId + ")");

		_state = FIELD;

		if (_def.Fields.Count == 0)
		{
		  return _next;
		}
		  }

		  return this;
		}

		internal override int depth()
		{
		  if (_state <= TYPE)
		  {
		return _next.depth();
		  }
		  else
		  {
		return _fieldDepth;
		  }
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case TYPE:
		return nextObject(ch);

		  case FIELD:
		if (_def.Fields.Count <= _count)
		{
		  return _next.next(ch);
		}

		_fieldDepth = _next.depth() + 2;
		println();
		print(_def.Fields[_count++] + ": ");

		_fieldDepth = outerInstance._column;

		outerInstance._isObject = false;
		return nextObject(ch);

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class ListState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TYPE = 0;
		internal const int LENGTH = 1;
		internal const int VALUE = 2;

		internal int _refId;

		internal int _state;
		internal bool _hasData;
		internal int _count;
		internal int _valueDepth;

		internal ListState(HessianDebugState outerInstance, State next, int refId, bool isType) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _refId = refId;

		  if (isType)
		  {
		_state = TYPE;
		  }
		  else
		  {
		printObject("list (#" + _refId + ")");
		_state = VALUE;
		  }
		}

		internal override bool isShift(object value)
		{
		  return _state == TYPE || _state == LENGTH;
		}

		internal override State shift(object @object)
		{
		  if (_state == TYPE)
		  {
		object type = @object;

		if (type is string)
		{
		  outerInstance._typeDefList.Add((string) type);
		}
		else if (@object is int?)
		{
		  int index = (int?) @object;

		  if (index >= 0 && index < outerInstance._typeDefList.Count)
		  {
			type = outerInstance._typeDefList[index];
		  }
		  else
		  {
			type = "type-unknown(" + index + ")";
		  }
		}

		printObject("list " + type + "(#" + _refId + ")");

		_state = VALUE;

		return this;
		  }
		  else if (_state == LENGTH)
		  {
		_state = VALUE;

		return this;
		  }
		  else
		  {
		return this;
		  }
		}

		internal override int depth()
		{
		  if (_state <= LENGTH)
		  {
		return _next.depth();
		  }
		  else if (_state == VALUE)
		  {
		return _valueDepth;
		  }
		  else
		  {
		return _next.depth() + 2;
		  }
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case TYPE:
		return nextObject(ch);

		  case VALUE:
		if (ch == 'Z')
		{
		  if (_count > 0)
		  {
			println();
		  }

		  return _next;
		}
		else
		{
		  _valueDepth = _next.depth() + 2;
		  println();
		  printObject(_count++ + ": ");
		  _valueDepth = outerInstance._column;
		  outerInstance._isObject = false;

		  return nextObject(ch);
		}

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class CompactListState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TYPE = 0;
		internal const int LENGTH = 1;
		internal const int VALUE = 2;

		internal int _refId;

		internal bool _isTyped;
		internal bool _isLength;

		internal int _state;
		internal bool _hasData;
		internal int _length;
		internal int _count;
		internal int _valueDepth;

		internal CompactListState(HessianDebugState outerInstance, State next, int refId, bool isTyped) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _isTyped = isTyped;
		  _refId = refId;

		  if (isTyped)
		  {
		_state = TYPE;
		  }
		  else
		  {
		_state = LENGTH;
		  }
		}

		internal CompactListState(HessianDebugState outerInstance, State next, int refId, bool isTyped, int length) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _isTyped = isTyped;
		  _refId = refId;
		  _length = length;

		  _isLength = true;

		  if (isTyped)
		  {
		_state = TYPE;
		  }
		  else
		  {
		printObject("list (#" + _refId + ")");

		_state = VALUE;
		  }
		}

		internal override bool isShift(object value)
		{
		  return _state == TYPE || _state == LENGTH;
		}

		internal override State shift(object @object)
		{
		  if (_state == TYPE)
		  {
		object type = @object;

		if (@object is int?)
		{
		  int index = (int?) @object;

		  if (index >= 0 && index < outerInstance._typeDefList.Count)
		  {
			type = outerInstance._typeDefList[index];
		  }
		  else
		  {
			type = "type-unknown(" + index + ")";
		  }
		}
		else if (@object is string)
		{
		  outerInstance._typeDefList.Add((string) @object);
		}

		printObject("list " + type + " (#" + _refId + ")");

		if (_isLength)
		{
		  _state = VALUE;

		  if (_length == 0)
		  {
			return _next;
		  }
		}
		else
		{
		  _state = LENGTH;
		}

		return this;
		  }
		  else if (_state == LENGTH)
		  {
		_length = (int?) @object;

		if (!_isTyped)
		{
		  printObject("list (#" + _refId + ")");
		}

		_state = VALUE;

		if (_length == 0)
		{
		  return _next;
		}
		else
		{
		  return this;
		}
		  }
		  else
		  {
		return this;
		  }
		}

		internal override int depth()
		{
		  if (_state <= LENGTH)
		  {
		return _next.depth();
		  }
		  else if (_state == VALUE)
		  {
		return _valueDepth;
		  }
		  else
		  {
		return _next.depth() + 2;
		  }
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case TYPE:
		return nextObject(ch);

		  case LENGTH:
		return nextObject(ch);

		  case VALUE:
		if (_length <= _count)
		{
		  return _next.next(ch);
		}
		else
		{
		  _valueDepth = _next.depth() + 2;
		  println();
		  printObject(_count++ + ": ");
		  _valueDepth = outerInstance._column;
		  outerInstance._isObject = false;

		  return nextObject(ch);
		}

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class Hessian2State : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int MAJOR = 0;
		internal const int MINOR = 1;

		internal int _state;
		internal int _major;
		internal int _minor;

		internal Hessian2State(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override int depth()
		{
		  return _next.depth() + 2;
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case MAJOR:
		_major = ch;
		_state = MINOR;
		return this;

		  case MINOR:
		_minor = ch;
		println(-2, "hessian " + _major + "." + _minor);
		return _next;

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class CallState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int MAJOR = 0;
		internal const int MINOR = 1;
		internal const int HEADER = 2;
		internal const int METHOD = 3;
		internal const int VALUE = 4;
		internal const int ARG = 5;

		internal int _state;
		internal int _major;
		internal int _minor;

		internal CallState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override int depth()
		{
		  return _next.depth() + 2;
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case MAJOR:
		_major = ch;
		_state = MINOR;
		return this;

		  case MINOR:
		_minor = ch;
		_state = HEADER;
		println(-2, "call " + _major + "." + _minor);
		return this;

		  case HEADER:
		if (ch == 'H')
		{
		  println();
		  print("header ");
		  outerInstance._isObject = false;
		  _state = VALUE;
		  return new StringState(outerInstance, this, 'H', true);
		}
		 else if (ch == 'm')
		 {
		  println();
		  print("method ");
		  outerInstance._isObject = false;
		  _state = ARG;
		  return new StringState(outerInstance, this, 'm', true);
		 }
		else
		{
		  println((char) ch + ": unexpected char");
		  return outerInstance.popStack();
		}

		  case VALUE:
		print(" => ");
		outerInstance._isObject = false;
		_state = HEADER;
		return nextObject(ch);

		  case ARG:
		if (ch == 'Z')
		{
		  return _next;
		}
		else
		{
		  return nextObject(ch);
		}

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class Call2State : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int METHOD = 0;
		internal const int COUNT = 1;
		internal const int ARG = 2;

		internal int _state = METHOD;
		internal int _i;
		internal int _count;

		internal Call2State(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override int depth()
		{
		  return _next.depth() + 5;
		}

		internal override bool isShift(object value)
		{
		  return _state != ARG;
		}

		internal override State shift(object @object)
		{
		  if (_state == METHOD)
		  {
		println(-5, "Call " + @object);

		_state = COUNT;
		return this;
		  }
		  else if (_state == COUNT)
		  {
		int? count = (int?) @object;

		_count = count.Value;

		_state = ARG;

		if (_count == 0)
		{
		  return _next;
		}
		else
		{
		  return this;
		}
		  }
		  else
		  {
		return this;
		  }
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case COUNT:
		return nextObject(ch);

		  case METHOD:
		return nextObject(ch);

		  case ARG:
		if (_count <= _i)
		{
		  return _next.next(ch);
		}
		else
		{
		  println();
		  print(-3, _i++ + ": ");

		  return nextObject(ch);
		}

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class ReplyState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int MAJOR = 0;
		internal const int MINOR = 1;
		internal const int HEADER = 2;
		internal const int VALUE = 3;
		internal const int END = 4;

		internal int _state;
		internal int _major;
		internal int _minor;

		internal ReplyState(HessianDebugState outerInstance, State next) : base(outerInstance)
		{
			this.outerInstance = outerInstance;
		  _next = next;
		}

		internal override int depth()
		{
		  return _next.depth() + 2;
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case MAJOR:
		if (ch == 't' || ch == 'S')
		{
		  return (new RemoteState(outerInstance, this)).next(ch);
		}

		_major = ch;
		_state = MINOR;
		return this;

		  case MINOR:
		_minor = ch;
		_state = HEADER;
		println(-2, "reply " + _major + "." + _minor);
		return this;

		  case HEADER:
		if (ch == 'H')
		{
		  _state = VALUE;
		  return new StringState(outerInstance, this, 'H', true);
		}
		else if (ch == 'f')
		{
		  print("fault ");
		  outerInstance._isObject = false;
		  _state = END;
		  return new MapState(outerInstance, this, 0);
		}
		 else
		 {
		  _state = END;
		  return nextObject(ch);
		 }

		  case VALUE:
		_state = HEADER;
		return nextObject(ch);

		  case END:
		println();
		if (ch == 'Z')
		{
		  return _next;
		}
		else
		{
		  return _next.next(ch);
		}

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class Reply2State : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal Reply2State(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  println(-2, "Reply");
		}

		internal override int depth()
		{
		  return _next.depth() + 2;
		}

		internal override State next(int ch)
		{
		  return nextObject(ch);
		}
	  }

	  internal class Fault2State : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal Fault2State(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  println(-2, "Fault");
		}

		internal override int depth()
		{
		  return _next.depth() + 2;
		}

		internal override State next(int ch)
		{
		  return nextObject(ch);
		}
	  }

	  internal class IndirectState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal IndirectState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override bool isShift(object @object)
		{
		  return _next.isShift(@object);
		}

		internal override State shift(object @object)
		{
		  return _next.shift(@object);
		}

		internal override State next(int ch)
		{
		  return nextObject(ch);
		}
	  }

	  internal class RemoteState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal const int TYPE = 0;
		internal const int VALUE = 1;
		internal const int END = 2;

		internal int _state;
		internal int _major;
		internal int _minor;

		internal RemoteState(HessianDebugState outerInstance, State next) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;
		}

		internal override State next(int ch)
		{
		  switch (_state)
		  {
		  case TYPE:
		println(-1, "remote");
		if (ch == 't')
		{
		  _state = VALUE;
		  return new StringState(outerInstance, this, 't', false);
		}
		else
		{
		  _state = END;
		  return nextObject(ch);
		}

		  case VALUE:
		_state = END;
		return _next.nextObject(ch);

		  case END:
		return _next.next(ch);

		  default:
		throw new System.InvalidOperationException();
		  }
		}
	  }

	  internal class StreamingState : State
	  {
		  private readonly HessianDebugState outerInstance;

		internal int _digit;
		internal int _length;
		internal bool _isLast;
		internal bool _isFirst = true;

		internal State _childState;

		internal StreamingState(HessianDebugState outerInstance, State next, bool isLast) : base(outerInstance, next)
		{
			this.outerInstance = outerInstance;

		  _isLast = isLast;
		  _childState = new InitialState(outerInstance);
		}

		internal override State next(int ch)
		{
		  if (_digit < 2)
		  {
		_length = 256 * _length + ch;
		_digit++;

		if (_digit == 2 && _length == 0 && _isLast)
		{
		  outerInstance._refId = 0;
		  return _next;
		}
		else
		{
		  if (_digit == 2)
		  {
			println(-1, "packet-start(" + _length + ")");
		  }

		  return this;
		}
		  }
		  else if (_length == 0)
		  {
		_isLast = (ch == 'P');
		_digit = 0;

		return this;
		  }

		  _childState = _childState.next(ch);

		  _length--;

		  if (_length == 0 && _isLast)
		  {
		println(-1, "");
		println(-1, "packet-end");
		outerInstance._refId = 0;
		return _next;
		  }
		  else
		  {
		return this;
		  }
		}
	  }

	  internal class ObjectDef
	  {
		internal string _type;
		internal List<string> _fields;

		internal ObjectDef(string type, List<string> fields)
		{
		  _type = type;
		  _fields = fields;
		}

		internal virtual string Type
		{
			get
			{
			  return _type;
			}
		}

		internal virtual List<string> Fields
		{
			get
			{
			  return _fields;
			}
		}
	  }
	}

}