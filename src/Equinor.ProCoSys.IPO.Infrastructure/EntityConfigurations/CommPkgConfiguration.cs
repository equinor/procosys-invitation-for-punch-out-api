using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.IPO.Infrastructure.EntityConfigurations
{
    internal class CommPkgConfiguration : IEntityTypeConfiguration<CommPkg>
    {
        public void Configure(EntityTypeBuilder<CommPkg> builder)
        {
            builder.ConfigurePlant();
            builder.ConfigureCreationAudit();
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.ProjectName)
                .HasMaxLength(Invitation.ProjectNameMaxLength)
                .IsRequired();

            builder.Property(x => x.CommPkgNo)
                .HasMaxLength(CommPkg.CommPkgNoMaxLength)
                .IsRequired();

            builder.Property(x => x.System)
                .HasMaxLength(CommPkg.SystemMaxLength)
                .IsRequired();
        }
    }
}
