using MechanicShop.Domain.Identity;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id).IsClustered(false);
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.RefreshTokenTokenMaxLength);

        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.ExpiresOnUtc)
            .IsRequired();

        builder.Property(rt => rt.IsConsumed)
            .IsRequired();

        builder.Property(rt => rt.ServerFingerprint)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.ServerFingerprintMaxLength);

        // Configure the owned entity for device information
        builder.OwnsOne(rt => rt.Device, device =>
        {
            device.Property(d => d.Identifier)
                .IsRequired()
                .HasMaxLength(ConfigurationConstants.DeviceIdentifierMaxLength);

            device.Property(d => d.UserAgent)
                .HasMaxLength(ConfigurationConstants.UserAgentMaxLength);

            device.Property(d => d.IpAddress)
                .HasMaxLength(ConfigurationConstants.IpAddressMaxLength);

            device.HasIndex(d => d.Identifier);
        });

        builder.HasIndex(rt => rt.UserId);
    }
}