using System;

namespace Hessian.Lite
{
    public static class DateTimeUtils
    {
        public static readonly DateTime UtcStartTime = new DateTime(1970, 1, 1);
        public static long TimeStamp(this DateTime dateTime)
        {
            var span = dateTime - UtcStartTime;
            return (long)span.TotalMilliseconds;
        }
    }
}
