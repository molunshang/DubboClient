using System;
using System.Collections;
using System.Reflection;

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
	/// Input stream for Hessian requests, deserializing objects using the
	/// java.io.Serialization protocol.
	/// 
	/// <para>HessianSerializerInput is unbuffered, so any client needs to provide
	/// its own buffering.
	/// 
	/// <h3>Serialization</h3>
	/// 
	/// <pre>
	/// InputStream is = new FileInputStream("test.xml");
	/// HessianOutput in = new HessianSerializerOutput(is);
	/// 
	/// Object obj = in.readObject();
	/// is.close();
	/// </pre>
	/// 
	/// <h3>Parsing a Hessian reply</h3>
	/// 
	/// <pre>
	/// InputStream is = ...; // from http connection
	/// HessianInput in = new HessianSerializerInput(is);
	/// String value;
	/// 
	/// in.startReply();         // read reply header
	/// value = in.readString(); // read string value
	/// in.completeReply();      // read reply footer
	/// </pre>
	/// </para>
	/// </summary>
	public class HessianSerializerInput : HessianInput
	{
	  /// <summary>
	  /// Creates a new Hessian input stream, initialized with an
	  /// underlying input stream.
	  /// </summary>
	  /// <param name="is"> the underlying input stream. </param>
	  public HessianSerializerInput(System.IO.Stream @is) : base(@is)
	  {
	  }

	  /// <summary>
	  /// Creates an uninitialized Hessian input stream.
	  /// </summary>
	  public HessianSerializerInput()
	  {
	  }

	  /// <summary>
	  /// Reads an object from the input stream.  cl is known not to be
	  /// a Map.
	  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected Object readObjectImpl(Class cl) throws java.io.IOException
	  protected internal virtual object readObjectImpl(Type cl)
	  {
		try
		{
		  object obj = cl.newInstance();

		  if (_refs == null)
		  {
			_refs = new ArrayList();
		  }
		  _refs.Add(obj);

		  Hashtable fieldMap = getFieldMap(cl);

		  int code = read();
		  for (; code >= 0 && code != 'z'; code = read())
		  {
			_peek = code;

			object key = readObject();

			Field field = (Field) fieldMap[key];

			if (field != null)
			{
			  object value = readObject(field.Type);
			  field.set(obj, value);
			}
			else
			{
			  object value = readObject();
			}
		  }

		  if (code != 'z')
		  {
			throw expect("map", code);
		  }

		  // if there's a readResolve method, call it
		  try
		  {
			Method method = cl.GetMethod("readResolve", new Type[0]);
			return method.invoke(obj, new object[0]);
		  }
		  catch (Exception)
		  {
		  }

		  return obj;
		}
		catch (IOException e)
		{
		  throw e;
		}
		catch (Exception e)
		{
		  throw new IOExceptionWrapper(e);
		}
	  }

	  /// <summary>
	  /// Creates a map of the classes fields.
	  /// </summary>
	  protected internal virtual Hashtable getFieldMap(Type cl)
	  {
		Hashtable fieldMap = new Hashtable();

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

			fieldMap[field.Name] = field;
		  }
		}

		return fieldMap;
	  }
	}

}