using System;
using System.Collections;
using System.Collections.Generic;
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
	/// Serializing an object for known object types.
	/// </summary>
	public class BeanSerializer : AbstractSerializer
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
	  private new static readonly Logger log = Logger.getLogger(typeof(BeanSerializer).FullName);

	  private static readonly object[] NULL_ARGS = new object[0];
	  private Method[] _methods;
	  private string[] _names;

	  private object _writeReplaceFactory;
	  private Method _writeReplace;

	  public BeanSerializer(Type cl, ClassLoader loader)
	  {
		introspectWriteReplace(cl, loader);

		ArrayList primitiveMethods = new ArrayList();
		ArrayList compoundMethods = new ArrayList();

		for (; cl != null; cl = cl.BaseType)
		{
		  Method[] methods = cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

		  for (int i = 0; i < methods.Length; i++)
		  {
		Method method = methods[i];

		if (Modifier.isStatic(method.Modifiers))
		{
		  continue;
		}

		if (method.ParameterTypes.length != 0)
		{
		  continue;
		}

		string name = method.Name;

		if (!name.StartsWith("get", StringComparison.Ordinal))
		{
		  continue;
		}

		Type type = method.ReturnType;

		if (type.Equals(typeof(void)))
		{
		  continue;
		}

		if (findSetter(methods, name, type) == null)
		{
		  continue;
		}

		// XXX: could parameterize the handler to only deal with public
		method.Accessible = true;

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		if (type.IsPrimitive || type.FullName.StartsWith("java.lang.", StringComparison.Ordinal) && !type.Equals(typeof(object)))
		{
		  primitiveMethods.Add(method);
		}
		else
		{
		  compoundMethods.Add(method);
		}
		  }
		}

		ArrayList methodList = new ArrayList();
		methodList.AddRange(primitiveMethods);
		methodList.AddRange(compoundMethods);

		methodList.Sort(new MethodNameCmp());

		_methods = new Method[methodList.Count];
		methodList.toArray(_methods);

		_names = new string[_methods.Length];

		for (int i = 0; i < _methods.Length; i++)
		{
		  string name = _methods[i].Name;

		  name = name.Substring(3);

		  int j = 0;
		  for (; j < name.Length && char.IsUpper(name[j]); j++)
		  {
		  }

		  if (j == 1)
		  {
		name = name.Substring(0, j).ToLower() + name.Substring(j);
		  }
		  else if (j > 1)
		  {
		name = name.Substring(0, j - 1).ToLower() + name.Substring(j - 1);
		  }

		  _names[i] = name;
		}
	  }

	  private void introspectWriteReplace(Type cl, ClassLoader loader)
	  {
		try
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  string className = cl.FullName + "HessianSerializer";

		  Type serializerClass = Type.GetType(className, false, loader);

		  object serializerObject = serializerClass.newInstance();

		  Method writeReplace = getWriteReplace(serializerClass, cl);

		  if (writeReplace != null)
		  {
		_writeReplaceFactory = serializerObject;
		_writeReplace = writeReplace;

		return;
		  }
		}
		catch (ClassNotFoundException)
		{
		}
		catch (Exception e)
		{
		  log.log(Level.FINER, e.ToString(), e);
		}

		_writeReplace = getWriteReplace(cl);
	  }

	  /// <summary>
	  /// Returns the writeReplace method
	  /// </summary>
	  protected internal virtual Method getWriteReplace(Type cl)
	  {
		for (; cl != null; cl = cl.BaseType)
		{
		  Method[] methods = cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

		  for (int i = 0; i < methods.Length; i++)
		  {
		Method method = methods[i];

		if (method.Name.Equals("writeReplace") && method.ParameterTypes.length == 0)
		{
		  return method;
		}
		  }
		}

		return null;
	  }

	  /// <summary>
	  /// Returns the writeReplace method
	  /// </summary>
	  protected internal virtual Method getWriteReplace(Type cl, Type param)
	  {
		for (; cl != null; cl = cl.BaseType)
		{
		  foreach (Method method in cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
		  {
		if (method.Name.Equals("writeReplace") && method.ParameterTypes.length == 1 && param.Equals(method.ParameterTypes[0]))
		{
		  return method;
		}
		  }
		}

		return null;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeObject(Object obj, AbstractHessianOutput out) throws java.io.IOException
	  public override void writeObject(object obj, AbstractHessianOutput @out)
	  {
		if (@out.addRef(obj))
		{
		  return;
		}

		Type cl = obj.GetType();

		try
		{
		  if (_writeReplace != null)
		  {
		object repl;

		if (_writeReplaceFactory != null)
		{
		  repl = _writeReplace.invoke(_writeReplaceFactory, obj);
		}
		else
		{
		  repl = _writeReplace.invoke(obj);
		}

		@out.removeRef(obj);

		@out.writeObject(repl);

		@out.replaceRef(repl, obj);

		return;
		  }
		}
		catch (Exception e)
		{
		  log.log(Level.FINER, e.ToString(), e);
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		int @ref = @out.writeObjectBegin(cl.FullName);

		if (@ref < -1)
		{
		  // Hessian 1.1 uses a map

		  for (int i = 0; i < _methods.Length; i++)
		  {
		Method method = _methods[i];
		object value = null;

		try
		{
		  value = _methods[i].invoke(obj, (object []) null);
		}
		catch (Exception e)
		{
		  log.log(Level.FINE, e.ToString(), e);
		}

		@out.WriteString(_names[i]);

		@out.writeObject(value);
		  }

		  @out.writeMapEnd();
		}
		else
		{
		  if (@ref == -1)
		  {
		@out.writeInt(_names.Length);

		for (int i = 0; i < _names.Length; i++)
		{
		  @out.WriteString(_names[i]);
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		@out.writeObjectBegin(cl.FullName);
		  }

		  for (int i = 0; i < _methods.Length; i++)
		  {
		Method method = _methods[i];
		object value = null;

		try
		{
		  value = _methods[i].invoke(obj, (object []) null);
		}
		catch (Exception e)
		{
		  log.log(Level.FINER, e.ToString(), e);
		}

		@out.writeObject(value);
		  }
		}
	  }

	  /// <summary>
	  /// Finds any matching setter.
	  /// </summary>
	  private Method findSetter(Method[] methods, string getterName, Type arg)
	  {
		string setterName = "set" + getterName.Substring(3);

		for (int i = 0; i < methods.Length; i++)
		{
		  Method method = methods[i];

		  if (!method.Name.Equals(setterName))
		  {
		continue;
		  }

		  if (!method.ReturnType.Equals(typeof(void)))
		  {
		continue;
		  }

		  Type[] @params = method.ParameterTypes;

		  if (@params.Length == 1 && @params[0].Equals(arg))
		  {
		return method;
		  }
		}

		return null;
	  }

	  internal class MethodNameCmp : IComparer<Method>
	  {
		public virtual int Compare(Method a, Method b)
		{
		  return a.Name.compareTo(b.Name);
		}
	  }
	}

}