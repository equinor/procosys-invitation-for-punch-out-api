using System;

namespace Equinor.ProCoSys.IPO.Domain.Time
{
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
