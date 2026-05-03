using MechanicShop.Domain.RepairTasks;
using MechanicShop.Infrastructure.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MechanicShop.Infrastructure.Data.Configurations;

public sealed class RepairTaskConfiguration : AuditableEntityConfiguration<RepairTask>
{
    public override void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        base.Configure(builder);
        builder.HasKey(rt => rt.Id).IsClustered(false);
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.NameMaxLength);

        builder.Property(rt => rt.LaborCost)
            .IsRequired()
            .HasPrecision(ConfigurationConstants.Precision, ConfigurationConstants.Scale);

        builder.Property(rt => rt.EstimatedRepairDurationMinutes)
            .IsRequired()
            .HasConversion<int>();

        builder.HasMany(rt => rt.Parts)
            .WithOne()
            // Shadow property for the foreign key, since Part doesn't have a navigation property back to RepairTask
            // Only accessable via EF Core internals, not exposed in the domain model
            // FK exists in the database but not in the Part class
            .HasForeignKey("RepairTaskId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(rt => rt.Parts)
          .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}