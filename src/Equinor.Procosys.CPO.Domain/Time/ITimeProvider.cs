using System;

namespace Equinor.Procosys.CPO.Domain.Time
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
