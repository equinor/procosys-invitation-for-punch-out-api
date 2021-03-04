using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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

            builder.Property(x => x.ProjectName)
                .HasMaxLength(Invitation.ProjectNameMaxLength)
                .IsRequired();

            builder.Property(x => x.McPkgNo)
                .IsRequired();

            builder.Property(x => x.CommPkgNo)
                .IsRequired();

            builder.Property(x => x.System)
                .IsRequired();
        }
    }
}
