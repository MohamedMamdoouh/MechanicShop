using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandHandler(
    IAppDbContext context,
    ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
    IWorkOrderService workOrderService,
    HybridCache cache)
    : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders
            .FirstOrDefaultAsync(wo => wo.Id == request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("Work order with ID {WorkOrderId} not found.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        if (!workOrder.IsEditable)
        {
            logger.LogWarning("Work order with ID {WorkOrderId} is read-only.", request.WorkOrderId);
            return ApplicationErrors.ReadonlyWorkOrder;
        }

        var repairTasks = await context.RepairTasks
            .Where(rt => request.RepairTaskIds.Contains(rt.Id))
            .ToListAsync(cancellationToken);

        if (repairTasks.Count != request.RepairTaskIds.Count)
        {
            var missingIds = request.RepairTaskIds.Except(repairTasks.Select(rt => rt.Id));

            logger.LogWarning(
                "Some repair tasks not found for work order ID {WorkOrderId}. Missing IDs: {MissingIds}",
                request.WorkOrderId,
                string.Join(", ", missingIds));

            return ApplicationErrors.RepairTaskNotFound;
        }

        var clearRepairTasksResult = workOrder.ClearRepairTasks();

        if (!clearRepairTasksResult.IsSuccess)
        {
            logger.LogError(
                "Failed to clear existing repair tasks for work order ID {WorkOrderId}. Error: {Error}",
                request.WorkOrderId,
                clearRepairTasksResult.TopError);

            return clearRepairTasksResult.Errors.ToList();
        }

        foreach (var task in repairTasks)
        {
            var addRepairTaskResult = workOrder.AddRepairTask(task);

            if (!addRepairTaskResult.IsSuccess)
            {
                logger.LogError(
                    "Failed to add repair task ID {RepairTaskId} to work order ID {WorkOrderId}. Error: {Error}",
                    task.Id,
                    request.WorkOrderId,
                    addRepairTaskResult.TopError);

                return addRepairTaskResult.Errors.ToList();
            }
        }

        var newEndAt = workOrderService.CalculateEndTime(workOrder.StartAtUtc, repairTasks);

        var isOutsideOperatingHours = workOrderService.IsOutsideOperatingHours(workOrder.StartAtUtc, newEndAt);

        if (isOutsideOperatingHours)
        {
            logger.LogWarning(
                "Work order ID {WorkOrderId} with new repair tasks would end outside of operating hours."
                + " Start: {StartAt}, End: {EndAt}",
                request.WorkOrderId,
                workOrder.StartAtUtc,
                newEndAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHours;
        }

        var isLaborOccupied = await workOrderService.IsLaborOccupiedAsync(
            workOrder.LaborId,
            workOrder.StartAtUtc,
            newEndAt,
            request.WorkOrderId,
            cancellationToken);

        if (isLaborOccupied)
        {
            logger.LogWarning(
                "Work order ID {WorkOrderId} with new repair tasks would have labor occupied."
                + " Start: {StartAt}, End: {EndAt}",
                request.WorkOrderId,
                workOrder.StartAtUtc,
                newEndAt);

            return ApplicationErrors.LaborOccupied;
        }

        var updateTimingResult = workOrder.UpdateTiming(workOrder.StartAtUtc, newEndAt);

        if (!updateTimingResult.IsSuccess)
        {
            logger.LogError(
                "Failed to update timing for work order ID {WorkOrderId}. Error: {Error}",
                request.WorkOrderId,
                updateTimingResult.TopError);

            return updateTimingResult.Errors.ToList();
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified(workOrder.Id));

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(CacheTags.WorkOrderById(request.WorkOrderId), cancellationToken);

        return Result.Updated;
    }
}