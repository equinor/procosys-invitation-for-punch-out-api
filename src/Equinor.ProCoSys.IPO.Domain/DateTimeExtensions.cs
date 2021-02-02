using System;

namespace Equinor.ProCoSys.IPO.Domain
{
    public static class DateTimeExtensions
    {
        private static readonly DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

        public static DateTime AddWeeks(this DateTime dateTime, int weeks)
            => dateTime.AddDays(7 * weeks);

        public static DateTime StartOfWeek(this DateTime dt)
        {
            var dayOfWeek = GetDayOfWeekMondayAsFirst(dt);
            var diff = -(dayOfWeek - (int)firstDayOfWeek);
            return dt.AddDays(diff).Date;
        }

        private static int GetDayOfWeekMondayAsFirst(DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Sunday)
            {
                // treat Sunday as last day of week since enum DayOfWeek has Sunday as 0
                return 7;
            }

            return (int)dt.DayOfWeek;
        }
    }
}
