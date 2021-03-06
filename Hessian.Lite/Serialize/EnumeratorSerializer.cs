﻿using System;
using System.Collections;

namespace Hessian.Lite.Serialize
{
    public class EnumeratorSerializer : AbstractSerializer
    {
        protected override void DoWrite(object obj, Hessian2Writer writer)
        {
            writer.WriteListStart(-1, null);
            var enumerator = (IEnumerator)obj;
            while (enumerator.MoveNext())
            {
                writer.WriteObject(enumerator.Current);
            }
            writer.WriteListEnd();
        }
    }
}