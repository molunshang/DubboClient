using System;
using System.IO;
using System.Xml;

namespace Hessian.IO
{


    /// <summary>
    /// Abstract base class for Hessian requests.  Hessian users should only
    /// need to use the methods in this class.
    /// 
    /// <pre>
    /// AbstractHessianInput in = ...; // get input
    /// String value;
    /// 
    /// in.startReply();         // read reply header
    /// value = in.readString(); // read string value
    /// in.completeReply();      // read reply footer
    /// </pre>
    /// </summary>
    public abstract class AbstractHessianInput
    {
        private HessianRemoteResolver resolver;

        /// <summary>
        /// Initialize the Hessian stream with the underlying input stream.
        /// </summary>
        public virtual void init(System.IO.Stream @is)
        {
        }

        /// <summary>
        /// Returns the call's method
        /// </summary>
        public abstract string Method { get; }

        /// <summary>
        /// Sets the resolver used to lookup remote objects.
        /// </summary>
        public virtual HessianRemoteResolver RemoteResolver
        {
            set
            {
                this.resolver = value;
            }
            get
            {
                return resolver;
            }
        }


        /// <summary>
        /// Sets the serializer factory.
        /// </summary>
        public virtual SerializerFactory SerializerFactory
        {
            set
            {
            }
        }

        /// <summary>
        /// Reads the call
        /// 
        /// <pre>
        /// c major minor
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract int readCall() throws java.io.IOException;
        public abstract int readCall();

        /// <summary>
        /// For backward compatibility with HessianSkeleton
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void skipOptionalCall() throws java.io.IOException
        public virtual void skipOptionalCall()
        {
        }

        /// <summary>
        /// Reads a header, returning null if there are no headers.
        /// 
        /// <pre>
        /// H b16 b8 value
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract String readHeader() throws java.io.IOException;
        public abstract string readHeader();

        /// <summary>
        /// Starts reading the call
        /// 
        /// <para>A successful completion will have a single value:
        /// 
        /// <pre>
        /// m b16 b8 method
        /// </pre>
        /// </para>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract String readMethod() throws java.io.IOException;
        public abstract string readMethod();

        /// <summary>
        /// Reads the number of method arguments
        /// </summary>
        /// <returns> -1 for a variable length (hessian 1.0) </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int readMethodArgLength() throws java.io.IOException
        public virtual int readMethodArgLength()
        {
            return -1;
        }

        /// <summary>
        /// Starts reading the call, including the headers.
        /// 
        /// <para>The call expects the following protocol data
        /// 
        /// <pre>
        /// c major minor
        /// m b16 b8 method
        /// </pre>
        /// </para>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void startCall() throws java.io.IOException;
        public abstract void startCall();

        /// <summary>
        /// Completes reading the call
        /// 
        /// <para>The call expects the following protocol data
        /// 
        /// <pre>
        /// Z
        /// </pre>
        /// </para>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void completeCall() throws java.io.IOException;
        public abstract void completeCall();

        /// <summary>
        /// Reads a reply as an object.
        /// If the reply has a fault, throws the exception.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract Object readReply(Class expectedClass) throws Throwable;
        public abstract object readReply(Type expectedClass);

        /// <summary>
        /// Starts reading the reply
        /// 
        /// <para>A successful completion will have a single value:
        /// 
        /// <pre>
        /// r
        /// v
        /// </pre>
        /// </para>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void startReply() throws Throwable;
        public abstract void startReply();

        /// <summary>
        /// Completes reading the call
        /// 
        /// <para>A successful completion will have a single value:
        /// 
        /// <pre>
        /// z
        /// </pre>
        /// </para>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void completeReply() throws java.io.IOException;
        public abstract void completeReply();

        /// <summary>
        /// Reads a boolean
        /// 
        /// <pre>
        /// T
        /// F
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract boolean readBoolean() throws java.io.IOException;
        public abstract bool readBoolean();

        /// <summary>
        /// Reads a null
        /// 
        /// <pre>
        /// N
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void readNull() throws java.io.IOException;
        public abstract void readNull();

        /// <summary>
        /// Reads an integer
        /// 
        /// <pre>
        /// I b32 b24 b16 b8
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract int readInt() throws java.io.IOException;
        public abstract int readInt();

        /// <summary>
        /// Reads a long
        /// 
        /// <pre>
        /// L b64 b56 b48 b40 b32 b24 b16 b8
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract long readLong() throws java.io.IOException;
        public abstract long readLong();

        /// <summary>
        /// Reads a double.
        /// 
        /// <pre>
        /// D b64 b56 b48 b40 b32 b24 b16 b8
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract double readDouble() throws java.io.IOException;
        public abstract double readDouble();

        /// <summary>
        /// Reads a date.
        /// 
        /// <pre>
        /// T b64 b56 b48 b40 b32 b24 b16 b8
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract long readUTCDate() throws java.io.IOException;
        public abstract long readUTCDate();

        /// <summary>
        /// Reads a string encoded in UTF-8
        /// 
        /// <pre>
        /// s b16 b8 non-final string chunk
        /// S b16 b8 final string chunk
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract String readString() throws java.io.IOException;
        public abstract string readString();

        /// <summary>
        /// Reads an XML node encoded in UTF-8
        /// 
        /// <pre>
        /// x b16 b8 non-final xml chunk
        /// X b16 b8 final xml chunk
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public org.w3c.dom.Node readNode() throws java.io.IOException
        public virtual XmlNode readNode()
        {
            throw new System.NotSupportedException(this.GetType().Name);
        }

        /// <summary>
        /// Starts reading a string.  All the characters must be read before
        /// calling the next method.  The actual characters will be read with
        /// the reader's read() or read(char [], int, int).
        /// 
        /// <pre>
        /// s b16 b8 non-final string chunk
        /// S b16 b8 final string chunk
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract java.io.Reader getReader() throws java.io.IOException;
        public abstract StreamReader Reader { get; }

        /// <summary>
        /// Starts reading a byte array using an input stream.  All the bytes
        /// must be read before calling the following method.
        /// 
        /// <pre>
        /// b b16 b8 non-final binary chunk
        /// B b16 b8 final binary chunk
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract java.io.InputStream readInputStream() throws java.io.IOException;
        public abstract System.IO.Stream readInputStream();

        /// <summary>
        /// Reads a byte array.
        /// 
        /// <pre>
        /// b b16 b8 non-final binary chunk
        /// B b16 b8 final binary chunk
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract byte [] readBytes() throws java.io.IOException;
        public abstract sbyte[] readBytes();

        /// <summary>
        /// Reads an arbitrary object from the input stream.
        /// </summary>
        /// <param name="expectedClass"> the expected class if the protocol doesn't supply it. </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract Object readObject(Class expectedClass) throws java.io.IOException;
        public abstract object readObject(Type expectedClass);

        /// <summary>
        /// Reads an arbitrary object from the input stream.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract Object readObject() throws java.io.IOException;
        public abstract object readObject();

        /// <summary>
        /// Reads a remote object reference to the stream.  The type is the
        /// type of the remote interface.
        /// 
        /// <code><pre>
        /// 'r' 't' b16 b8 type url
        /// </pre></code>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract Object readRemote() throws java.io.IOException;
        public abstract object readRemote();

        /// <summary>
        /// Reads a reference
        /// 
        /// <pre>
        /// R b32 b24 b16 b8
        /// </pre>
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract Object readRef() throws java.io.IOException;
        public abstract object readRef();

        /// <summary>
        /// Adds an object reference.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract int addRef(Object obj) throws java.io.IOException;
        public abstract int addRef(object obj);

        /// <summary>
        /// Sets an object reference.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void setRef(int i, Object obj) throws java.io.IOException;
        public abstract void setRef(int i, object obj);

        /// <summary>
        /// Resets the references for streaming.
        /// </summary>
        public virtual void resetReferences()
        {
        }

        /// <summary>
        /// Reads the start of a list
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract int readListStart() throws java.io.IOException;
        public abstract int readListStart();

        /// <summary>
        /// Reads the length of a list.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract int readLength() throws java.io.IOException;
        public abstract int readLength();

        /// <summary>
        /// Reads the start of a map
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract int readMapStart() throws java.io.IOException;
        public abstract int readMapStart();

        /// <summary>
        /// Reads an object type.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract String readType() throws java.io.IOException;
        public abstract string readType();

        /// <summary>
        /// Returns true if the data has ended.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract boolean isEnd() throws java.io.IOException;
        public abstract bool End { get; }

        /// <summary>
        /// Read the end byte
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void readEnd() throws java.io.IOException;
        public abstract void readEnd();

        /// <summary>
        /// Read the end byte
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void readMapEnd() throws java.io.IOException;
        public abstract void readMapEnd();

        /// <summary>
        /// Read the end byte
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void readListEnd() throws java.io.IOException;
        public abstract void readListEnd();

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void close() throws java.io.IOException
        public virtual void close()
        {
        }
    }

}