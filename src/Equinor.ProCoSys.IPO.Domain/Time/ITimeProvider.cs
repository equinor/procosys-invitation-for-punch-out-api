using System;

namespace Equinor.ProCoSys.IPO.Domain.Time
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
