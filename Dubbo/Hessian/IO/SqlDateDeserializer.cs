﻿using System;

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
	/// Deserializing a string valued object
	/// </summary>
	public class SqlDateDeserializer : AbstractDeserializer
	{
	  private Type _cl;
	  private Constructor _constructor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SqlDateDeserializer(Class cl) throws NoSuchMethodException
	  public SqlDateDeserializer(Type cl)
	  {
		_cl = cl;
		_constructor = cl.GetConstructor(new Type[] {typeof(long)});
	  }

	  public override Type Type
	  {
		  get
		  {
			return _cl;
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readMap(AbstractHessianInput in) throws java.io.IOException
	  public override object readMap(AbstractHessianInput @in)
	  {
		int @ref = @in.addRef(null);

		long initValue = long.MinValue;

		while (!@in.End)
		{
		  string key = @in.readString();

		  if (key.Equals("value"))
		  {
		initValue = @in.readUTCDate();
		  }
		  else
		  {
		@in.readString();
		  }
		}

		@in.readMapEnd();

		object value = create(initValue);

		@in.setRef(@ref, value);

		return value;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readObject(AbstractHessianInput in, String [] fieldNames) throws java.io.IOException
	  public override object readObject(AbstractHessianInput @in, string[] fieldNames)
	  {
		int @ref = @in.addRef(null);

		long initValue = long.MinValue;

		for (int i = 0; i < fieldNames.Length; i++)
		{
		  string key = fieldNames[i];

		  if (key.Equals("value"))
		  {
		initValue = @in.readUTCDate();
		  }
		  else
		  {
		@in.readObject();
		  }
		}

		object value = create(initValue);

		@in.setRef(@ref, value);

		return value;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Object create(long initValue) throws java.io.IOException
	  private object create(long initValue)
	  {
		if (initValue == long.MinValue)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new IOException(_cl.FullName + " expects name.");
		}

		try
		{
		  return _constructor.newInstance(new object[] {new long?(initValue)});
		}
		catch (Exception e)
		{
		  throw new IOExceptionWrapper(e);
		}
	  }
	}

}