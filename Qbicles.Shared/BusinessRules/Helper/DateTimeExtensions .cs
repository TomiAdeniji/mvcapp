using System;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.BusinessRules
{
    public static partial class DateTimeExtensions
    {
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt) =>
            dt.FirstDayOfWeek().AddDays(6);

        public static DateTime FirstDayOfMonth(this DateTime dt) =>
            new DateTime(dt.Year, dt.Month, 1);

        public static DateTime LastDayOfMonth(this DateTime dt) =>
            dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);

        public static DateTime FirstDayOfNextMonth(this DateTime dt) =>
            dt.FirstDayOfMonth().AddMonths(1);
        /// <summary>
        /// Support show formats with suffix
        /// eg : 1st Nov 2020, 1:45am
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="format">string format eg: ddnn MMMM yyyy, hh:mmtt</param>
        /// <param name="useExtendedSpecifiers">True: Show formats with suffix; False: Do not use the suffix </param>
        /// <returns></returns>
        public static string ToString(this DateTime dateTime, string format, bool useExtendedSpecifiers)
        {
            return useExtendedSpecifiers
                ? dateTime.ToString(format)
                    .Replace("nn", Converter.OrdinalSuffix(dateTime.Day).ToLower())
                    .Replace("NN", Converter.OrdinalSuffix(dateTime.Day).ToUpper())
                : dateTime.ToString(format);
        }
    }
}
