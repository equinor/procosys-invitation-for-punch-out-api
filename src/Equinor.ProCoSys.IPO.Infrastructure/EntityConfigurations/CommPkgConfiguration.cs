using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
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
            
            builder.HasIndex(x => x.Guid)
                .IsUnique();

            builder.HasOne<Project>()
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.CommPkgNo)
                .HasMaxLength(CommPkg.CommPkgNoMaxLength)
                .IsRequired();

            builder.Property(x => x.System)
                .HasMaxLength(CommPkg.SystemMaxLength)
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
