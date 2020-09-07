using Equinor.Procosys.CPO.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.Procosys.CPO.Infrastructure.EntityConfigurations.Extensions
{
    public static class PlantConfigurationExtensions
    {
        public static void ConfigurePlant<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : PlantEntityBase =>
            builder.Property(x => x.Plant)
                .HasMaxLength(PlantEntityBase.PlantLengthMax)
                .IsRequired();
    }
}
