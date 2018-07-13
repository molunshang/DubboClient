using System;

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
    public class BasicSerializer : AbstractSerializer
    {
        public const int NULL = 0;
        public const int BOOLEAN = NULL + 1;
        public const int BYTE = BOOLEAN + 1;
        public const int SHORT = BYTE + 1;
        public const int INTEGER = SHORT + 1;
        public const int LONG = INTEGER + 1;
        public const int FLOAT = LONG + 1;
        public const int DOUBLE = FLOAT + 1;
        public const int CHARACTER = DOUBLE + 1;
        public const int CHARACTER_OBJECT = CHARACTER + 1;
        public const int STRING = CHARACTER_OBJECT + 1;
        public const int DATE = STRING + 1;
        public const int NUMBER = DATE + 1;
        public const int OBJECT = NUMBER + 1;

        public const int BOOLEAN_ARRAY = OBJECT + 1;
        public const int BYTE_ARRAY = BOOLEAN_ARRAY + 1;
        public const int SHORT_ARRAY = BYTE_ARRAY + 1;
        public const int INTEGER_ARRAY = SHORT_ARRAY + 1;
        public const int LONG_ARRAY = INTEGER_ARRAY + 1;
        public const int FLOAT_ARRAY = LONG_ARRAY + 1;
        public const int DOUBLE_ARRAY = FLOAT_ARRAY + 1;
        public const int CHARACTER_ARRAY = DOUBLE_ARRAY + 1;
        public const int STRING_ARRAY = CHARACTER_ARRAY + 1;
        public const int OBJECT_ARRAY = STRING_ARRAY + 1;

        private int code;

        public BasicSerializer(int code)
        {
            this.code = code;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void writeObject(Object obj, AbstractHessianOutput out) throws java.io.IOException
        public override void writeObject(object obj, AbstractHessianOutput @out)
        {
            switch (code)
            {
                case BOOLEAN:
                    @out.writeBoolean(((bool?)obj).Value);
                    break;

                case BYTE:
                    @out.writeInt((byte)obj);
                    break;
                case SHORT:
                    @out.writeInt((short)obj);
                    break;
                case INTEGER:
                    @out.writeInt((int)obj);
                    break;

                case LONG:
                    @out.writeLong((long)obj);
                    break;

                case FLOAT:
                case DOUBLE:
                    @out.writeDouble((double)obj);
                    break;

                case CHARACTER:
                case CHARACTER_OBJECT:
                    @out.WriteString(obj.ToString());
                    break;

                case STRING:
                    @out.WriteString((string)obj);
                    break;

                case DATE:
                    @out.writeUTCDate(((DateTime)obj).Ticks);
                    break;

                case BOOLEAN_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        bool[] data = (bool[])obj;
                        bool hasEnd = @out.writeListBegin(data.Length, "[boolean");
                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeBoolean(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }

                        break;
                    }

                case BYTE_ARRAY:
                    {
                        sbyte[] data = (sbyte[])obj;
                        @out.writeBytes(data, 0, data.Length);
                        break;
                    }

                case SHORT_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        short[] data = (short[])obj;
                        bool hasEnd = @out.writeListBegin(data.Length, "[short");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeInt(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }
                        break;
                    }

                case INTEGER_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        int[] data = (int[])obj;

                        bool hasEnd = @out.writeListBegin(data.Length, "[int");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeInt(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }

                        break;
                    }

                case LONG_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        long[] data = (long[])obj;

                        bool hasEnd = @out.writeListBegin(data.Length, "[long");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeLong(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }
                        break;
                    }

                case FLOAT_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        float[] data = (float[])obj;

                        bool hasEnd = @out.writeListBegin(data.Length, "[float");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeDouble(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }
                        break;
                    }

                case DOUBLE_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        double[] data = (double[])obj;
                        bool hasEnd = @out.writeListBegin(data.Length, "[double");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeDouble(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }
                        break;
                    }

                case STRING_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        string[] data = (string[])obj;

                        bool hasEnd = @out.writeListBegin(data.Length, "[string");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.WriteString(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }
                        break;
                    }

                case CHARACTER_ARRAY:
                    {
                        char[] data = (char[])obj;
                        @out.WriteString(data, 0, data.Length);
                        break;
                    }

                case OBJECT_ARRAY:
                    {
                        if (@out.addRef(obj))
                        {
                            return;
                        }

                        object[] data = (object[])obj;

                        bool hasEnd = @out.writeListBegin(data.Length, "[object");

                        for (int i = 0; i < data.Length; i++)
                        {
                            @out.writeObject(data[i]);
                        }

                        if (hasEnd)
                        {
                            @out.writeListEnd();
                        }
                        break;
                    }

                case NULL:
                    @out.writeNull();
                    break;

                default:
                    throw new Exception(code + " " + obj.GetType().ToString());
            }
        }
    }

}