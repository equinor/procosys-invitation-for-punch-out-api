﻿using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations
{
    internal class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.ConfigurePlant();
            builder.ConfigureCreationAudit();
            builder.ConfigureModificationAudit();
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.ProjectName)
                .HasMaxLength(Invitation.ProjectNameMaxLength)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(Invitation.TitleMaxLength)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(Invitation.DescriptionMaxLength);

            builder.Property(x => x.Location)
                .HasMaxLength(Invitation.LocationMaxLength);

            builder
                .HasMany(x => x.McPkgs)
                .WithOne()
                .IsRequired();

            builder
                .HasMany(x => x.CommPkgs)
                .WithOne()
                .IsRequired();

            builder
                .HasMany(x => x.Participants)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(x => x.Comments)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(x => x.Attachments)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.StartTimeUtc)
                .IsRequired()
                .HasConversion(IPOContext.DateTimeKindConverter);

            builder.Property(x => x.EndTimeUtc)
                .IsRequired()
                .HasConversion(IPOContext.DateTimeKindConverter);

            builder.Property(x => x.CompletedAtUtc)
                .HasConversion(IPOContext.DateTimeKindConverter);

            builder
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.CompletedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.AcceptedAtUtc)
                .HasConversion(IPOContext.DateTimeKindConverter);

            builder
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.AcceptedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasIndex(i => i.Plant)
                .HasFilter("[Status] <> 3")
                .IncludeProperties(i => new { i.Description, i.Status });

            builder
               .HasIndex("Plant", "ProjectName")
               .IncludeProperties(i => new
               {
                   i.Title,
                   i.Description,
                   i.Type,
                   i.CompletedAtUtc,
                   i.AcceptedAtUtc,
                   i.StartTimeUtc,
                   i.RowVersion,
                   i.Status
               });         
        }
    }
}
