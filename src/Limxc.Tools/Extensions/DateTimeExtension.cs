using System;
using System.Collections.Generic;

namespace Limxc.Tools.Extensions
{
    public static class DateTimeExtension
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     TimeFromUnixTimestamp
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long unixTimestamp)
        {
            var unixTimeStampInTicks = unixTimestamp * TimeSpan.TicksPerMillisecond;
            return new DateTime(_epoch.Ticks + unixTimeStampInTicks);
        }

        /// <summary>
        ///     UnixTimestampFromDateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime dateTime)
        {
            var unixTimestamp = dateTime.Ticks - _epoch.Ticks;
            unixTimestamp /= TimeSpan.TicksPerMillisecond;
            return unixTimestamp;
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                ? DateTimeOffset.MinValue
                : new DateTimeOffset(dateTime);
        }

        /// <summary>
        ///     获取年龄
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static (int Year, int Month, int Day) Age(this DateTime begin, DateTime end)
        {
            Dictionary<int, int> MonthDay;
            var ts = end - begin;
            int year = 0, month = 0, day = 0;

            if (end.Year % 4 == 0)
                MonthDay = new Dictionary<int, int>
                {
                    {1, 31}, {2, 29}, {3, 31}, {4, 30}, {5, 31}, {6, 30}, {7, 31}, {8, 31}, {9, 30}, {10, 31}, {11, 30},
                    {12, 31}
                };
            else
                MonthDay = new Dictionary<int, int>
                {
                    {1, 31}, {2, 28}, {3, 31}, {4, 30}, {5, 31}, {6, 30}, {7, 31}, {8, 31}, {9, 30}, {10, 31}, {11, 30},
                    {12, 31}
                };

            //year
            year = end.Year - begin.Year;
            if (end.Month < begin.Month)
                year--;

            if (end.Month == begin.Month)
                if (end.Day < begin.Day)
                    year--;

            //month
            month = end.Month - begin.Month;
            if (month <= 0)
                month += 12;

            if (end.Day < begin.Day)
                month--;
            else if (month == 12)
                month = 0;

            //day
            if (end.Day < begin.Day)
                day = end.Day + MonthDay[begin.Month] - begin.Day;
            else
                day = end.Day - begin.Day;

            return (year, month, day);
        }
    }
}