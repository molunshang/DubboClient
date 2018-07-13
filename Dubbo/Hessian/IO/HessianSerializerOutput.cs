using System;
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
	/// Output stream for Hessian requests.
	/// 
	/// <para>HessianOutput is unbuffered, so any client needs to provide
	/// its own buffering.
	/// 
	/// <h3>Serialization</h3>
	/// 
	/// <pre>
	/// OutputStream os = new FileOutputStream("test.xml");
	/// HessianOutput out = new HessianSerializerOutput(os);
	/// 
	/// out.writeObject(obj);
	/// os.close();
	/// </pre>
	/// 
	/// <h3>Writing an RPC Call</h3>
	/// 
	/// <pre>
	/// OutputStream os = ...; // from http connection
	/// HessianOutput out = new HessianSerializerOutput(os);
	/// String value;
	/// 
	/// out.startCall("hello");  // start hello call
	/// out.writeString("arg1"); // write a string argument
	/// out.completeCall();      // complete the call
	/// </pre>
	/// </para>
	/// </summary>
	public class HessianSerializerOutput : HessianOutput
	{
	  /// <summary>
	  /// Creates a new Hessian output stream, initialized with an
	  /// underlying output stream.
	  /// </summary>
	  /// <param name="os"> the underlying output stream. </param>
	  public HessianSerializerOutput(System.IO.Stream os) : base(os)
	  {
	  }

	  /// <summary>
	  /// Creates an uninitialized Hessian output stream.
	  /// </summary>
	  public HessianSerializerOutput()
	  {
	  }

	  /// <summary>
	  /// Applications which override this can do custom serialization.
	  /// </summary>
	  /// <param name="object"> the object to write. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObjectImpl(Object obj) throws java.io.IOException
	  public virtual void writeObjectImpl(object obj)
	  {
		Type cl = obj.GetType();

		try
		{
		  Method method = cl.GetMethod("writeReplace", new Type[0]);
		  object repl = method.invoke(obj, new object[0]);

		  writeObject(repl);
		  return;
		}
		catch (Exception)
		{
		}

		try
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  writeMapBegin(cl.FullName);
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

			  WriteString(field.Name);
			  writeObject(field.get(obj));
			}
		  }
		  writeMapEnd();
		}
		catch (IllegalAccessException e)
		{
		  throw new IOExceptionWrapper(e);
		}
	  }
	}

}