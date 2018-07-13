using System;
using System.Collections.Generic;

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
	/// Deserializing a JDK 1.2 Class.
	/// </summary>
	public class ClassDeserializer : AbstractMapDeserializer
	{
	  private static readonly Dictionary<string, Type> _primClasses = new Dictionary<string, Type>();

	  private ClassLoader _loader;

	  public ClassDeserializer(ClassLoader loader)
	  {
		_loader = loader;
	  }

	  public override Type Type
	  {
		  get
		  {
			return typeof(Type);
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readMap(AbstractHessianInput in) throws java.io.IOException
	  public override object readMap(AbstractHessianInput @in)
	  {
		int @ref = @in.addRef(null);

		string name = null;

		while (!@in.End)
		{
		  string key = @in.readString();

		  if (key.Equals("name"))
		  {
		name = @in.readString();
		  }
		  else
		  {
		@in.readObject();
		  }
		}

		@in.readMapEnd();

		object value = create(name);

		@in.setRef(@ref, value);

		return value;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in, String [] fieldNames) throws java.io.IOException
	  public override object readObject(AbstractHessianInput @in, string[] fieldNames)
	  {
		int @ref = @in.addRef(null);

		string name = null;

		for (int i = 0; i < fieldNames.Length; i++)
		{
		  if ("name".Equals(fieldNames[i]))
		  {
			name = @in.readString();
		  }
		  else
		  {
		@in.readObject();
		  }
		}

		object value = create(name);

		@in.setRef(@ref, value);

		return value;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object create(String name) throws java.io.IOException
	  internal virtual object create(string name)
	  {
		if (string.ReferenceEquals(name, null))
		{
		  throw new IOException("Serialized Class expects name.");
		}

		Type cl = _primClasses[name];

		if (cl != null)
		{
		  return cl;
		}

		try
		{
		  if (_loader != null)
		  {
			return Type.GetType(name, false, _loader);
		  }
		  else
		  {
			return Type.GetType(name);
		  }
		}
		catch (Exception e)
		{
		  throw new IOExceptionWrapper(e);
		}
	  }

	  static ClassDeserializer()
	  {
		_primClasses["void"] = typeof(void);
		_primClasses["boolean"] = typeof(bool);
		_primClasses["java.lang.Boolean"] = typeof(Boolean);
		_primClasses["byte"] = typeof(sbyte);
		_primClasses["java.lang.Byte"] = typeof(Byte);
		_primClasses["char"] = typeof(char);
		_primClasses["java.lang.Character"] = typeof(Character);
		_primClasses["short"] = typeof(short);
		_primClasses["java.lang.Short"] = typeof(Short);
		_primClasses["int"] = typeof(int);
		_primClasses["java.lang.Integer"] = typeof(Integer);
		_primClasses["long"] = typeof(long);
		_primClasses["java.lang.Long"] = typeof(Long);
		_primClasses["float"] = typeof(float);
		_primClasses["java.lang.Float"] = typeof(Float);
		_primClasses["double"] = typeof(double);
		_primClasses["java.lang.Double"] = typeof(Double);
		_primClasses["java.lang.String"] = typeof(string);
	  }
	}

}