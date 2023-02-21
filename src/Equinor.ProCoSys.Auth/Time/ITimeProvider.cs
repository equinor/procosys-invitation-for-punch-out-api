using System;

namespace Equinor.ProCoSys.Auth.Time
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
