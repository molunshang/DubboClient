using System;

namespace Dubbo.Remote
{
    public class RemotingException : Exception
    {
        public RemotingException()
        {

        }

        public RemotingException(string msg) : base(msg)
        {

        }

        public RemotingException(string msg, Exception ex) : base(msg, ex)
        {

        }
    }
}