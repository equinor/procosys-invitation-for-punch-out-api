using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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

            builder
                .HasMany(x => x.McPkgs)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(x => x.CommPkgs)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

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

            builder.Property(x => x.StartTimeUtc)
                .IsRequired()
                .HasConversion(IPOContext.DateTimeKindConverter);

            builder.Property(x => x.EndTimeUtc)
                .IsRequired()
                .HasConversion(IPOContext.DateTimeKindConverter);
        }
    }
}
