using System;
using System.Collections;

namespace Hessian.IO
{


    /// <summary>
    /// Serializing an object for known object types.
    /// </summary>
    public class BasicDeserializer : AbstractDeserializer
    {
        public const int NULL = BasicSerializer.NULL;
        public const int BOOLEAN = BasicSerializer.BOOLEAN;
        public const int BYTE = BasicSerializer.BYTE;
        public const int SHORT = BasicSerializer.SHORT;
        public const int INTEGER = BasicSerializer.INTEGER;
        public const int LONG = BasicSerializer.LONG;
        public const int FLOAT = BasicSerializer.FLOAT;
        public const int DOUBLE = BasicSerializer.DOUBLE;
        public const int CHARACTER = BasicSerializer.CHARACTER;
        public const int CHARACTER_OBJECT = BasicSerializer.CHARACTER_OBJECT;
        public const int STRING = BasicSerializer.STRING;
        public const int DATE = BasicSerializer.DATE;
        public const int NUMBER = BasicSerializer.NUMBER;
        public const int OBJECT = BasicSerializer.OBJECT;

        public const int BOOLEAN_ARRAY = BasicSerializer.BOOLEAN_ARRAY;
        public const int BYTE_ARRAY = BasicSerializer.BYTE_ARRAY;
        public const int SHORT_ARRAY = BasicSerializer.SHORT_ARRAY;
        public const int INTEGER_ARRAY = BasicSerializer.INTEGER_ARRAY;
        public const int LONG_ARRAY = BasicSerializer.LONG_ARRAY;
        public const int FLOAT_ARRAY = BasicSerializer.FLOAT_ARRAY;
        public const int DOUBLE_ARRAY = BasicSerializer.DOUBLE_ARRAY;
        public const int CHARACTER_ARRAY = BasicSerializer.CHARACTER_ARRAY;
        public const int STRING_ARRAY = BasicSerializer.STRING_ARRAY;
        public const int OBJECT_ARRAY = BasicSerializer.OBJECT_ARRAY;

        private int _code;

        public BasicDeserializer(int code)
        {
            _code = code;
        }

        public override Type Type
        {
            get
            {
                switch (_code)
                {
                    case NULL:
                        return typeof(void);
                    case BOOLEAN:
                        return typeof(bool);
                    case BYTE:
                        return typeof(byte);
                    case SHORT:
                        return typeof(short);
                    case INTEGER:
                        return typeof(int);
                    case LONG:
                        return typeof(long);
                    case FLOAT:
                        return typeof(float);
                    case DOUBLE:
                        return typeof(double);
                    case CHARACTER:
                    case CHARACTER_OBJECT:
                        return typeof(char);
                    case STRING:
                        return typeof(string);
                    case DATE:
                        return typeof(DateTime);
                    case NUMBER:
                        throw new NotImplementedException();
                    case OBJECT:
                        return typeof(object);

                    case BOOLEAN_ARRAY:
                        return typeof(bool[]);
                    case BYTE_ARRAY:
                        return typeof(sbyte[]);
                    case SHORT_ARRAY:
                        return typeof(short[]);
                    case INTEGER_ARRAY:
                        return typeof(int[]);
                    case LONG_ARRAY:
                        return typeof(long[]);
                    case FLOAT_ARRAY:
                        return typeof(float[]);
                    case DOUBLE_ARRAY:
                        return typeof(double[]);
                    case CHARACTER_ARRAY:
                        return typeof(char[]);
                    case STRING_ARRAY:
                        return typeof(string[]);
                    case OBJECT_ARRAY:
                        return typeof(object[]);
                    default:
                        throw new System.NotSupportedException();
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readObject(AbstractHessianInput in) throws java.io.IOException
        public override object readObject(AbstractHessianInput @in)
        {
            switch (_code)
            {
                case NULL:
                    // hessian/3490
                    @in.readObject();

                    return null;

                case BOOLEAN:
                    return Convert.ToBoolean(@in.readBoolean());

                case BYTE:
                    return Convert.ToSByte((sbyte)@in.readInt());

                case SHORT:
                    return Convert.ToInt16((short)@in.readInt());

                case INTEGER:
                    return Convert.ToInt32(@in.readInt());

                case LONG:
                    return Convert.ToInt64(@in.readLong());

                case FLOAT:
                    return Convert.ToSingle((float)@in.readDouble());

                case DOUBLE:
                    return Convert.ToDouble(@in.readDouble());

                case STRING:
                    return @in.readString();

                case OBJECT:
                    return @in.readObject();

                case CHARACTER:
                    {
                        string s = @in.readString();
                        if (string.ReferenceEquals(s, null) || s.Equals(""))
                        {
                            return Convert.ToChar((char)0);
                        }
                        else
                        {
                            return Convert.ToChar(s[0]);
                        }
                    }

                    goto case CHARACTER_OBJECT;
                case CHARACTER_OBJECT:
                    {
                        string s = @in.readString();
                        if (string.ReferenceEquals(s, null) || s.Equals(""))
                        {
                            return null;
                        }
                        else
                        {
                            return Convert.ToChar(s[0]);
                        }
                    }

                    goto case DATE;
                case DATE:
                    return new DateTime(@in.readUTCDate());

                case NUMBER:
                    return @in.readObject();

                case BYTE_ARRAY:
                    return @in.readBytes();

                case CHARACTER_ARRAY:
                    {
                        string s = @in.readString();

                        if (string.ReferenceEquals(s, null))
                        {
                            return null;
                        }
                        else
                        {
                            int len = s.Length;
                            char[] chars = new char[len];
                            s.CopyTo(0, chars, 0, len - 0);
                            return chars;
                        }
                    }

                    goto case BOOLEAN_ARRAY;
                case BOOLEAN_ARRAY:
                case SHORT_ARRAY:
                case INTEGER_ARRAY:
                case LONG_ARRAY:
                case FLOAT_ARRAY:
                case DOUBLE_ARRAY:
                case STRING_ARRAY:
                    {
                        int code = @in.readListStart();

                        switch (code)
                        {
                            case 'N':
                                return null;

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
                                int length = code - 0x10;
                                @in.readInt();

                                return readLengthList(@in, length);

                            default:
                                string type = @in.readType();
                                length = @in.readLength();

                                return readList(@in, length);
                        }
                    }

                    goto default;
                default:
                    throw new System.NotSupportedException();
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readList(AbstractHessianInput in, int length) throws java.io.IOException
        public override object readList(AbstractHessianInput @in, int length)
        {
            switch (_code)
            {
                case BOOLEAN_ARRAY:
                    {
                        if (length >= 0)
                        {
                            bool[] data = new bool[length];

                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = @in.readBoolean();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(Convert.ToBoolean(@in.readBoolean()));
                            }

                            @in.readEnd();

                            bool[] data = new bool[list.Count];

                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = ((bool?)list[i]).Value;
                            }

                            return data;
                        }
                    }

                    goto case SHORT_ARRAY;
                case SHORT_ARRAY:
                    {
                        if (length >= 0)
                        {
                            short[] data = new short[length];

                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (short)@in.readInt();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(Convert.ToInt16((short)@in.readInt()));
                            }

                            @in.readEnd();

                            short[] data = new short[list.Count];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = ((short?)list[i]).Value;
                            }

                            @in.addRef(data);

                            return data;
                        }
                    }

                    goto case INTEGER_ARRAY;
                case INTEGER_ARRAY:
                    {
                        if (length >= 0)
                        {
                            int[] data = new int[length];

                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = @in.readInt();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(Convert.ToInt32(@in.readInt()));
                            }


                            @in.readEnd();

                            int[] data = new int[list.Count];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = ((int?)list[i]).Value;
                            }

                            @in.addRef(data);

                            return data;
                        }
                    }

                    goto case LONG_ARRAY;
                case LONG_ARRAY:
                    {
                        if (length >= 0)
                        {
                            long[] data = new long[length];

                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = @in.readLong();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(Convert.ToInt64(@in.readLong()));
                            }

                            @in.readEnd();

                            long[] data = new long[list.Count];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = ((long?)list[i]).Value;
                            }

                            @in.addRef(data);

                            return data;
                        }
                    }

                    goto case FLOAT_ARRAY;
                case FLOAT_ARRAY:
                    {
                        if (length >= 0)
                        {
                            float[] data = new float[length];
                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (float)@in.readDouble();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(new float?((float)@in.readDouble()));
                            }

                            @in.readEnd();

                            float[] data = new float[list.Count];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = ((float?)list[i]).Value;
                            }

                            @in.addRef(data);

                            return data;
                        }
                    }

                    goto case DOUBLE_ARRAY;
                case DOUBLE_ARRAY:
                    {
                        if (length >= 0)
                        {
                            double[] data = new double[length];
                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = @in.readDouble();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(new double?(@in.readDouble()));
                            }

                            @in.readEnd();

                            double[] data = new double[list.Count];
                            @in.addRef(data);
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = ((double?)list[i]).Value;
                            }

                            return data;
                        }
                    }

                    goto case STRING_ARRAY;
                case STRING_ARRAY:
                    {
                        if (length >= 0)
                        {
                            string[] data = new string[length];
                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = @in.readString();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            while (!@in.End)
                            {
                                list.Add(@in.readString());
                            }

                            @in.readEnd();

                            string[] data = new string[list.Count];
                            @in.addRef(data);
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (string)list[i];
                            }

                            return data;
                        }
                    }

                    goto case OBJECT_ARRAY;
                case OBJECT_ARRAY:
                    {
                        if (length >= 0)
                        {
                            object[] data = new object[length];
                            @in.addRef(data);

                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = @in.readObject();
                            }

                            @in.readEnd();

                            return data;
                        }
                        else
                        {
                            ArrayList list = new ArrayList();

                            @in.addRef(list); // XXX: potential issues here

                            while (!@in.End)
                            {
                                list.Add(@in.readObject());
                            }

                            @in.readEnd();

                            object[] data = new object[list.Count];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (object)list[i];
                            }

                            return data;
                        }
                    }

                    goto default;
                default:
                    throw new System.NotSupportedException(this.ToString());
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object readLengthList(AbstractHessianInput in, int length) throws java.io.IOException
        public override object readLengthList(AbstractHessianInput @in, int length)
        {
            switch (_code)
            {
                case BOOLEAN_ARRAY:
                    {
                        bool[] data = new bool[length];

                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = @in.readBoolean();
                        }

                        return data;
                    }

                case SHORT_ARRAY:
                    {
                        short[] data = new short[length];

                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = (short)@in.readInt();
                        }

                        return data;
                    }

                case INTEGER_ARRAY:
                    {
                        int[] data = new int[length];

                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = @in.readInt();
                        }

                        return data;
                    }

                case LONG_ARRAY:
                    {
                        long[] data = new long[length];

                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = @in.readLong();
                        }

                        return data;
                    }

                case FLOAT_ARRAY:
                    {
                        float[] data = new float[length];
                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = (float)@in.readDouble();
                        }

                        return data;
                    }

                case DOUBLE_ARRAY:
                    {
                        double[] data = new double[length];
                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = @in.readDouble();
                        }

                        return data;
                    }

                case STRING_ARRAY:
                    {
                        string[] data = new string[length];
                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = @in.readString();
                        }

                        return data;
                    }

                case OBJECT_ARRAY:
                    {
                        object[] data = new object[length];
                        @in.addRef(data);

                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = @in.readObject();
                        }

                        return data;
                    }

                default:
                    throw new System.NotSupportedException(this.ToString());
            }
        }
    }

}