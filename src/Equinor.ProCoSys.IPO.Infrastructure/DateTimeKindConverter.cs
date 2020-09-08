using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Equinor.ProCoSys.IPO.Infrastructure
{
    public class DateTimeKindConverter : ValueConverter<DateTime, DateTime>
    {
        public DateTimeKindConverter() : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {}
    }
}
