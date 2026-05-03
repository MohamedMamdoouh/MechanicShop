using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class WorkOrderConfiguration : AuditableEntityConfiguration<WorkOrder>
{
    public override void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        base.Configure(builder);
        builder.HasKey(wo => wo.Id).IsClustered(false);
        builder.Property(wo => wo.Id).ValueGeneratedNever();

        builder.Property(wo => wo.LaborId).IsRequired();

        builder.HasOne(wo => wo.Labor)
            .WithMany()
            .HasForeignKey(wo => wo.LaborId)
            .IsRequired();

        builder.HasOne(wo => wo.Invoice)
            .WithOne(i => i.WorkOrder)
            .HasForeignKey<Invoice>(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wo => wo.Status)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.EnumMaxLength)
            .HasConversion<string>();

        builder.Property(wo => wo.Spot)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.EnumMaxLength)
            .HasConversion<string>();

        builder.Property(wo => wo.StartAtUtc)
            .IsRequired();

        builder.Property(wo => wo.EndAtUtc)
            .IsRequired();

        builder.Property(wo => wo.TaxPercentage)
            .IsRequired()
            .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);

        builder.Ignore(wo => wo.Tax);

        builder.Property(wo => wo.Discount)
            .IsRequired()
            .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);

        builder.Ignore(wo => wo.TotalCost);
        builder.Ignore(wo => wo.TotalPartsCost);
        builder.Ignore(wo => wo.TotalLaborCost);

        builder.HasMany(wo => wo.RepairTasks)
            .WithMany()
            .UsingEntity(x => x.ToTable("WorkOrderRepairTasks"));

        builder.HasOne(wo => wo.Vehicle)
            .WithMany()
            .HasForeignKey(wo => wo.VehicleId);

        builder.HasIndex(wo => wo.VehicleId);
        builder.HasIndex(wo => wo.LaborId);
        builder.HasIndex(wo => wo.Status);
        builder.HasIndex(wo => new { wo.StartAtUtc, wo.EndAtUtc });
        builder.HasIndex(wo => new { wo.LaborId, wo.StartAtUtc, wo.EndAtUtc });
        builder.HasIndex(wo => new { wo.VehicleId, wo.StartAtUtc, wo.EndAtUtc });
        builder.HasIndex(wo => new { wo.LaborId, wo.Status, wo.StartAtUtc, wo.EndAtUtc });
    }
}