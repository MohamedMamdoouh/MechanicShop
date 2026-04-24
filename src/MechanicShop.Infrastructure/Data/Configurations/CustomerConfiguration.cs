
using MechanicShop.Domain.Customers;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id).IsClustered(false);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.NameMaxLength);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.NameMaxLength);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.EmailMaxLength);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(ConfigurationConstants.PhoneNumberMaxLength);

        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();

        builder.HasMany(c => c.Vehicles)
            .WithOne(v => v.Customer)
            .HasForeignKey(v => v.CustomerId);
    }
}