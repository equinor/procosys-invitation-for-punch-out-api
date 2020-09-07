using System;

namespace Equinor.Procosys.CPO.Domain
{
    public static class TimeSpanExtensions
    {
        public static int Weeks(this TimeSpan span) => span.Days / 7;
    }
}
