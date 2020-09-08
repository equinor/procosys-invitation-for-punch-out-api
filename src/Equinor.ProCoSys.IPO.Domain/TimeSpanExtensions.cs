using System;

namespace Equinor.ProCoSys.IPO.Domain
{
    public static class TimeSpanExtensions
    {
        public static int Weeks(this TimeSpan span) => span.Days / 7;
    }
}
