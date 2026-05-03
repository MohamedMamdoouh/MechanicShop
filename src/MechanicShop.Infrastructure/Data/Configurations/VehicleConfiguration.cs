using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class VehicleConfiguration : AuditableEntityConfiguration<Vehicle>
{
    public override void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        base.Configure(builder);
        builder.HasKey(v => v.Id).IsClustered(false);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.MakeMaxLength);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.ModelMaxLength);

        builder.Property(v => v.Year)
            .IsRequired();

        builder.Property(v => v.LicensePlate)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.LicensePlateMaxLength);

        builder.HasIndex(v => v.LicensePlate)
            .IsUnique();
    }
}