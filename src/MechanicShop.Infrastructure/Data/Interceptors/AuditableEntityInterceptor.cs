using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.BaseEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace MechanicShop.Infrastructure.Data.Interceptors;

public sealed class AuditableEntityInterceptor(IUser user, TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var utcNow = timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Unchanged && !entry.HasChangedOwnEntities())
            {
                continue;
            }

            ApplyAudit(entry, utcNow);
        }
    }

    private void ApplyAudit(EntityEntry<AuditableEntity> entry, DateTimeOffset utcNow)
    {
        var userId = string.IsNullOrWhiteSpace(user.Id) ? null : user.Id;

        if (entry.State == EntityState.Added)
        {
            entry.Property(e => e.CreatedBy).CurrentValue = userId;
            entry.Property(e => e.CreatedAtUtc).CurrentValue = utcNow;
        }

        entry.Property(e => e.LastModifiedBy).CurrentValue = userId;
        entry.Property(e => e.LastModifiedAt).CurrentValue = utcNow;
    }
}

public static class Extensions
{
    public static bool HasChangedOwnEntities(this EntityEntry entry)
    {
        return entry.References.Any(r =>
           r.TargetEntry?.Metadata.IsOwned() == true &&
           (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}