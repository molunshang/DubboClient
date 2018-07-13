﻿using System;
using System.Collections;

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
	/// Deserializing a JDK 1.2 Map.
	/// </summary>
	public class MapDeserializer : AbstractMapDeserializer
	{
	  private Type _type;
	  private Constructor _ctor;

	  public MapDeserializer(Type type)
	  {
		if (type == null)
		{
		  type = typeof(Hashtable);
		}

		_type = type;

		Constructor[] ctors = type.GetConstructors();
		for (int i = 0; i < ctors.Length; i++)
		{
		  if (ctors[i].ParameterTypes.length == 0)
		  {
		_ctor = ctors[i];
		  }
		}

		if (_ctor == null)
		{
		  try
		  {
		_ctor = typeof(Hashtable).GetConstructor(new Type[0]);
		  }
		  catch (Exception e)
		  {
		throw new System.InvalidOperationException(e);
		  }
		}
	  }

	  public override Type Type
	  {
		  get
		  {
			if (_type != null)
			{
			  return _type;
			}
			else
			{
			  return typeof(Hashtable);
			}
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object readMap(AbstractHessianInput in) throws java.io.IOException
	  public override object readMap(AbstractHessianInput @in)
	  {
		IDictionary map;

		if (_type == null)
		{
		  map = new Hashtable();
		}
		else if (_type.Equals(typeof(IDictionary)))
		{
		  map = new Hashtable();
		}
		else if (_type.Equals(typeof(SortedDictionary)))
		{
		  map = new SortedDictionary();
		}
		else
		{
		  try
		  {
			map = (IDictionary) _ctor.newInstance();
		  }
		  catch (Exception e)
		  {
			throw new IOExceptionWrapper(e);
		  }
		}

		@in.addRef(map);

		while (!@in.End)
		{
		  map[@in.readObject()] = @in.readObject();
		}

		@in.readEnd();

		return map;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object readObject(AbstractHessianInput in, String [] fieldNames) throws java.io.IOException
	  public override object readObject(AbstractHessianInput @in, string[] fieldNames)
	  {
		IDictionary map = createMap();

		int @ref = @in.addRef(map);

		for (int i = 0; i < fieldNames.Length; i++)
		{
		  string name = fieldNames[i];

		  map[name] = @in.readObject();
		}

		return map;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Map createMap() throws java.io.IOException
	  private IDictionary createMap()
	  {

		if (_type == null)
		{
		  return new Hashtable();
		}
		else if (_type.Equals(typeof(IDictionary)))
		{
		  return new Hashtable();
		}
		else if (_type.Equals(typeof(SortedDictionary)))
		{
		  return new SortedDictionary();
		}
		else
		{
		  try
		  {
			return (IDictionary) _ctor.newInstance();
		  }
		  catch (Exception e)
		  {
			throw new IOExceptionWrapper(e);
		  }
		}
	  }
	}

}