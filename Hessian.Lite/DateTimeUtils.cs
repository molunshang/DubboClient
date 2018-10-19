using System;

namespace Hessian.Lite
{
    public static class DateTimeUtils
    {
        public static readonly DateTime UtcStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long TimeStamp(this DateTime dateTime)
        {
            return (UtcStartTime.Ticks - dateTime.Ticks) / 10000;
        }
    }
}
