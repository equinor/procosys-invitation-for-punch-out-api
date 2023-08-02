﻿using Equinor.ProCoSys.IPO.Domain.AggregateModels.SettingAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations
{
    internal class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.Code)
                .HasMaxLength(Setting.CodeLengthMax)
                .IsRequired();

            builder.Property(x => x.Value)
                .HasMaxLength(Setting.ValueLengthMax)
                .IsRequired();
        }
    }
}
