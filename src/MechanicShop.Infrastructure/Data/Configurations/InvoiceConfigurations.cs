using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class InvoiceConfigurations : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id).IsClustered(false);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.IssuedAt)
            .IsRequired();

        builder.Property(x => x.DiscountAmount)
            .IsRequired()
            .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);

        builder.Property(x => x.TaxAmount)
            .IsRequired()
            .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);

        builder.Property(x => x.PaidAt)
            .IsRequired(false);

        builder.Property(x => x.PaymentStatus)
            .HasMaxLength(ConfigurationConstants.EnumMaxLength)
            .IsRequired()
            .HasConversion<string>();

        builder.Navigation(x => x.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.LineItems, items =>
        {
            items.ToTable("InvoiceLineItems");

            items.WithOwner().HasForeignKey(x => x.InvoiceId);

            items.HasKey(x => new { x.LineNumber, x.InvoiceId });

            items.Property(x => x.LineNumber).ValueGeneratedNever();

            items.Property(x => x.Description).IsRequired()
                .HasMaxLength(ConfigurationConstants.DescriptionMaxLength);

            items.Property(x => x.Quantity).IsRequired();

            items.Property(x => x.UnitPrice).IsRequired()
                .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);
        });

        builder.HasIndex("WorkOrderId");
    }
}