﻿using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations
{
    internal class HistoryConfiguration : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.ConfigurePlant();
            builder.ConfigureCreationAudit();
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.Description)
                .HasMaxLength(History.DescriptionLengthMax)
                .IsRequired();

            builder.Property(f => f.EventType)
                .HasConversion<string>()
                .IsRequired();

            builder
                .HasIndex(p => p.SourceGuid)
                .HasDatabaseName("IX_History_SourceGuid_ASC");

            builder.HasCheckConstraint("constraint_history_check_valid_event_type",
                $"{nameof(History.EventType)} in ({GetValidEventTypes()})");
        }

        private string GetValidEventTypes()
        {
            var names = Enum.GetNames(typeof(EventType)).Select(t => $"'{t}'");
            return string.Join(',', names);
        }
    }
}
