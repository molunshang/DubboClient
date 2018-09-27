using System;
using System.Collections.Generic;
using System.IO;

namespace Dubbo.Hessian
{
    public class Hessian2Output
    {
        private Stream dataStream;

        public Hessian2Output(Stream dataStream)
        {
            this.dataStream = dataStream;
        }

        internal void WriteString(string v)
        {
            throw new NotImplementedException();
        }

        internal void WriteObject(object arg)
        {
            throw new NotImplementedException();
        }
    }
}