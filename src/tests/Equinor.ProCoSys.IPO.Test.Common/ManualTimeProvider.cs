﻿using System;
using Equinor.ProCoSys.Auth.Time;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public class ManualTimeProvider : ITimeProvider
    {
        public ManualTimeProvider()
        {
        }

        public ManualTimeProvider(DateTime now)
        {
            if (now.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Must be UTC");
            }

            UtcNow = now;
        }

        public DateTime UtcNow { get; private set; }

        public void Elapse(TimeSpan elapsedTime) => UtcNow += elapsedTime;

        public void ElapseWeeks(int weeks) => Elapse(TimeSpan.FromDays(weeks * 7));

        public void SetTime(DateTime now) => UtcNow = now;
    }
}
