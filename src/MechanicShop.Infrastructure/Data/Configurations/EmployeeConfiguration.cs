using MechanicShop.Domain.Employees;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.NameMaxLength);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.NameMaxLength);

        builder.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(ConfigurationConstants.EnumMaxLength)
            .IsRequired();
    }
}