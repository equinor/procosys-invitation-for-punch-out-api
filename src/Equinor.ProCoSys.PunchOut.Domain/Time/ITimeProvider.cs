using System;

namespace Equinor.Procosys.PunchOut.Domain.Time
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
