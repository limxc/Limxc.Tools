using System;

namespace Limxc.Tools.Extensions
{
    public static class DateTimeExtension
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// TimeFromUnixTimestamp
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long unixTimestamp)
        {
            long unixTimeStampInTicks = unixTimestamp * TimeSpan.TicksPerMillisecond;
            return new DateTime(_epoch.Ticks + unixTimeStampInTicks);
        }

        /// <summary>
        /// UnixTimestampFromDateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime dateTime)
        {
            long unixTimestamp = dateTime.Ticks - _epoch.Ticks;
            unixTimestamp /= TimeSpan.TicksPerMillisecond;
            return unixTimestamp;
        }

        /// <summary>
        /// yyyyMMddHHmmss + ffff
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="msLength"></param>
        /// <returns></returns>
        public static string Format(this DateTime dateTime, int msLength = 0)
        {
            return $"{dateTime:yyyyMMddHHmmssffff}".Substring(0, msLength + 14);
        }
    }
}