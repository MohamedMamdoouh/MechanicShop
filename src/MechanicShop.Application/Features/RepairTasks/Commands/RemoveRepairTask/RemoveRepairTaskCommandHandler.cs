using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;

public sealed class RemoveRepairTaskCommandHandler(
    IAppDbContext context,
    ILogger<RemoveRepairTaskCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RemoveRepairTaskCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(RemoveRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await context.RepairTasks.FindAsync([request.RepairTaskId], cancellationToken);

        if (repairTask is null)
        {
            logger.LogWarning("Repair task not found with ID: {Id}", request.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }

        var isInUse = await context.WorkOrders.AsNoTracking()
        .SelectMany(x => x.RepairTasks)
        .AnyAsync(rt => rt.Id == request.RepairTaskId, cancellationToken);

        if (isInUse)
        {
            logger.LogWarning(
                "Cannot remove repair task with ID {RepairTaskId} because it is associated with a work order.",
                request.RepairTaskId);
            return ApplicationErrors.RepairTaskInUse;
        }

        context.RepairTasks.Remove(repairTask);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(CacheTags.RepairTasks, cancellationToken);
        logger.LogInformation("Repair task with ID {RepairTaskId} removed.", request.RepairTaskId);

        return Result.Deleted;
    }
}