using System;

namespace Equinor.Procosys.PunchOut.Domain.Time
{
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
