using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class PartConfiguration : AuditableEntityConfiguration<Part>
{
    public override void Configure(EntityTypeBuilder<Part> builder)
    {
        base.Configure(builder);
        builder.HasKey(p => p.Id).IsClustered(false);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.NameMaxLength);

        builder.Property(p => p.Cost)
            .IsRequired()
            .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);

        builder.Property(p => p.Quantity).IsRequired();
    }
}