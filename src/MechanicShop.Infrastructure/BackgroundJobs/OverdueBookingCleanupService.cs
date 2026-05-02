using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace MechanicShop.Infrastructure.BackgroundJobs;

public class OverdueBookingCleanupService(
    ILogger<OverdueBookingCleanupService> logger,
    IOptions<AppSettings> options,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.OverdueBookingCleanupFrequencyInMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Starting overdue booking cleanup at {Time}.", timeProvider.GetUtcNow());

            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

                if (!await TryAcquireAppLockAsync(context, stoppingToken))
                {
                    logger.LogInformation("Cleanup lock held by another instance. Skipping this run.");
                    continue;
                }

                var cutoff = timeProvider.GetUtcNow().AddMinutes(-options.Value.BookingCancellationThresholdInMinutes);

                var overdueWorkOrders = await context.WorkOrders
                    .Where(x => x.Status == WorkOrderState.Scheduled && x.StartAtUtc <= cutoff)
                    .ToListAsync(stoppingToken);

                if (overdueWorkOrders.Count == 0)
                {
                    logger.LogInformation("No overdue work orders found.");
                    continue;
                }

                foreach (var workOrder in overdueWorkOrders)
                {
                    var result = workOrder.Cancel(timeProvider.GetUtcNow());

                    if (!result.IsSuccess)
                    {
                        logger.LogWarning(
                            "Failed to cancel overdue work order with ID {WorkOrderId}: {ErrorMessage}",
                            workOrder.Id,
                            result.TopError);
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
                logger.LogInformation("Marked {Count} work orders as overdue.", overdueWorkOrders.Count);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Operation was cancelled.");
            }
            catch (Exception ex) when (ex is TimeoutException or DbUpdateException)
            {
                logger.LogError(ex, "Transient failure during cleanup.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unexpected error in cleanup service.");
            }
        }
    }

    private static async Task<bool> TryAcquireAppLockAsync(
        IAppDbContext context, CancellationToken cancellationToken)
    {
        const string Sql =
            "DECLARE @Result INT; " +
            "EXEC @Result = sp_getapplock " +
            "@Resource = N'OverdueBookingCleanup', " +
            "@LockMode = N'Exclusive', " +
            "@LockOwner = N'Transaction', " +
            "@LockTimeout = 0; " +
            "SELECT @Result;";

        var result = await context.Database
            .SqlQueryRaw<int>(Sql)
            .FirstOrDefaultAsync(cancellationToken);

        return result >= 0;
    }
}
