using System;
using System.Collections;
using System.Reflection;

namespace Hessian.IO
{


    /// <summary>
    /// Serializing an object for known object types.
    /// </summary>
    public class BeanDeserializer : AbstractMapDeserializer
    {
        private Type _type;
        private Hashtable _methodMap;
        private MethodInfo _readResolve;
        private ConstructorInfo _constructor;
        private object[] _constructorArgs;

        public BeanDeserializer(Type cl)
        {
            _type = cl;
            _methodMap = getMethodMap(cl);


            _readResolve = getReadResolve(cl);

            ConstructorInfo[] constructors = cl.GetConstructors();
            int bestLength = int.MaxValue;

            for (int i = 0; i < constructors.Length; i++)
            {
                var parameterTypes = constructors[i].GetParameters();
                if (parameterTypes.Length < bestLength)
                {
                    _constructor = constructors[i];
                    bestLength = _constructor.ParameterTypes.length;
                }
            }

            if (_constructor != null)
            {
                _constructor.Accessible = true;
                Type[] @params = _constructor.ParameterTypes;
                _constructorArgs = new object[@params.Length];
                for (int i = 0; i < @params.Length; i++)
                {
                    _constructorArgs[i] = getParamArg(@params[i]);
                }
            }
        }

        public override Type Type
        {
            get
            {
                return _type;
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readMap(AbstractHessianInput in) throws java.io.IOException
        public override object readMap(AbstractHessianInput @in)
        {
            try
            {
                object obj = instantiate();

                return readMap(@in, obj);
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

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readMap(AbstractHessianInput in, Object obj) throws java.io.IOException
        public virtual object readMap(AbstractHessianInput @in, object obj)
        {
            try
            {
                int @ref = @in.addRef(obj);

                while (!@in.End)
                {
                    object key = @in.readObject();

                    Method method = (Method)_methodMap[key];

                    if (method != null)
                    {
                        object value = @in.readObject(method.ParameterTypes[0]);

                        method.invoke(obj, new object[] { value });
                    }
                    else
                    {
                        object value = @in.readObject();
                    }
                }

                @in.readMapEnd();

                object resolve = resolve(obj);

                if (obj != resolve)
                {
                    @in.setRef(@ref, resolve);
                }

                return resolve;
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

        private object resolve(object obj)
        {
            // if there's a readResolve method, call it
            try
            {
                if (_readResolve != null)
                {
                    return _readResolve.invoke(obj, new object[0]);
                }
            }
            catch (Exception)
            {
            }

            return obj;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected Object instantiate() throws Exception
        protected internal virtual object instantiate()
        {
            return _constructor.newInstance(_constructorArgs);
        }

        /// <summary>
        /// Returns the readResolve method
        /// </summary>
        protected internal virtual Method getReadResolve(Type cl)
        {
            for (; cl != null; cl = cl.BaseType)
            {
                Method[] methods = cl.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                for (int i = 0; i < methods.Length; i++)
                {
                    Method method = methods[i];

                    if (method.Name.Equals("readResolve") && method.ParameterTypes.length == 0)
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a map of the classes fields.
        /// </summary>
        protected internal virtual Hashtable getMethodMap(Type cl)
        {
            Hashtable methodMap = new Hashtable();

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

                    string name = method.Name;

                    if (!name.StartsWith("set", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Type[] paramTypes = method.ParameterTypes;
                    if (paramTypes.Length != 1)
                    {
                        continue;
                    }

                    if (!method.ReturnType.Equals(typeof(void)))
                    {
                        continue;
                    }

                    if (findGetter(methods, name, paramTypes[0]) == null)
                    {
                        continue;
                    }

                    // XXX: could parameterize the handler to only deal with public
                    try
                    {
                        method.Accessible = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

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


                    methodMap[name] = method;
                }
            }

            return methodMap;
        }

        /// <summary>
        /// Finds any matching setter.
        /// </summary>
        private Method findGetter(Method[] methods, string setterName, Type arg)
        {
            string getterName = "get" + setterName.Substring(3);

            for (int i = 0; i < methods.Length; i++)
            {
                Method method = methods[i];

                if (!method.Name.Equals(getterName))
                {
                    continue;
                }

                if (!method.ReturnType.Equals(arg))
                {
                    continue;
                }

                Type[] @params = method.ParameterTypes;

                if (@params.Length == 0)
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a map of the classes fields.
        /// </summary>
        protected internal static object getParamArg(Type cl)
        {
            if (!cl.IsPrimitive)
            {
                return null;
            }
            else if (typeof(bool).Equals(cl))
            {
                return false;
            }
            else if (typeof(sbyte).Equals(cl))
            {
                return Convert.ToSByte((sbyte)0);
            }
            else if (typeof(short).Equals(cl))
            {
                return Convert.ToInt16((short)0);
            }
            else if (typeof(char).Equals(cl))
            {
                return Convert.ToChar((char)0);
            }
            else if (typeof(int).Equals(cl))
            {
                return Convert.ToInt32(0);
            }
            else if (typeof(long).Equals(cl))
            {
                return Convert.ToInt64(0);
            }
            else if (typeof(float).Equals(cl))
            {
                return Convert.ToDouble(0);
            }
            else if (typeof(double).Equals(cl))
            {
                return Convert.ToDouble(0);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }
    }

}