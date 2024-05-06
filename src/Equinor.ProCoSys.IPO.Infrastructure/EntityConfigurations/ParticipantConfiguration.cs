using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations
{
    internal class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
    {
        public void Configure(EntityTypeBuilder<Participant> builder)
        {
            builder.ConfigurePlant();
            builder.ConfigureCreationAudit();
            builder.ConfigureModificationAudit();
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.Organization)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.SortKey)
                .IsRequired();

            builder.HasIndex(x => x.Guid)
                .IsUnique();

            builder.Property(x => x.SignedAtUtc)
                .HasConversion(IPOContext.DateTimeKindConverter);

            builder
                .HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.SignedBy)
                .OnDelete(DeleteBehavior.NoAction);

            //Made specifically for Me/GetOutstandingIPOs
            builder
                .HasIndex("InvitationId", "Plant", "AzureOid")
                .IncludeProperties(p => new
                {
                    p.FunctionalRoleCode,
                    p.Organization,
                    p.SignedAtUtc,
                    p.SortKey,
                    p.Type,
                    p.SignedBy
                });
        }
    }
}
