﻿using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations
{
    internal class McPkgConfiguration : IEntityTypeConfiguration<McPkg>
    {
        public void Configure(EntityTypeBuilder<McPkg> builder)
        {
            builder.ConfigurePlant();
            builder.ConfigureCreationAudit();
            builder.ConfigureConcurrencyToken();

            builder.HasOne<Project>()
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.McPkgNo)
                .HasMaxLength(McPkg.McPkgNoMaxLength)
                .IsRequired();

            builder.Property(x => x.CommPkgNo)
                .HasMaxLength(CommPkg.CommPkgNoMaxLength)
                .IsRequired();

            builder.Property(x => x.System)
                .HasMaxLength(McPkg.SystemMaxLength)
                .IsRequired();

            builder
              .HasIndex("Plant", "InvitationId")
              .IncludeProperties(c => new
              {
                  c.CommPkgNo
              });
        }
    }
}
