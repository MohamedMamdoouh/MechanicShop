using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandHandler(
    IAppDbContext context,
    IWorkOrderService workOrderService,
    ILogger<RelocateWorkOrderCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        RelocateWorkOrderCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders
        .Include(x => x.RepairTasks)
        .Include(x => x.Labor)
        .Include(x => x.Vehicle)
        .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("Work order with id {WorkOrderId} not found", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        var workOrderDuration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();
        var newEndAt = request.NewStartAt.Add(workOrderDuration);

        var isSpotAvailable = await workOrderService.IsSpotAvailableAsync(
            request.Spot,
            request.NewStartAt,
            newEndAt,
            cancellationToken);

        if (!isSpotAvailable)
        {
            logger.LogWarning(
                "Spot {Spot} is not available from {NewStartAt} to {NewEndAt}",
                request.Spot,
                request.NewStartAt,
                newEndAt);

            return ApplicationErrors.WorkOrderSpotNotAvailable;
        }

        var isLaborOccupied = await workOrderService.IsLaborOccupiedAsync(
            workOrder!.LaborId,
            request.NewStartAt,
            newEndAt,
            workOrder.Id,
            cancellationToken);

        if (isLaborOccupied)
        {
            logger.LogWarning(
                "Labor with id {LaborId} is already occupied during the new time slot",
                workOrder.LaborId);

            return ApplicationErrors.LaborOccupied;
        }

        var isVehicleScheduled = await workOrderService.IsVehicleAlreadyScheduledAsync(
            workOrder.VehicleId,
            request.NewStartAt,
            newEndAt,
            workOrder.Id,
            cancellationToken);

        if (isVehicleScheduled)
        {
            logger.LogWarning(
                "Vehicle with id {VehicleId} is already scheduled during the new time slot",
                workOrder.VehicleId);

            return ApplicationErrors.VehicleSchedulingConflict;
        }

        var updateTimingReslt = workOrder.UpdateTiming(
            request.NewStartAt,
            newEndAt);

        if (!updateTimingReslt.IsSuccess)
        {
            logger.LogWarning(
                "Failed to update timing for work order with id {WorkOrderId}: {Error}",
                workOrder.Id,
                updateTimingReslt.Errors);

            return updateTimingReslt.Errors.ToList();
        }

        var updateSpotResult = workOrder.UpdateSpot(request.Spot);

        if (!updateSpotResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to update spot for work order with id {WorkOrderId}: {Error}",
                workOrder.Id,
                updateSpotResult.Errors);

            return updateSpotResult.Errors.ToList();
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified(workOrder.Id));

        context.WorkOrders.Update(workOrder);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Successfully relocated work order with id {WorkOrderId} to new spot {Spot}"
            + " and new timing from {NewStartAt} to {NewEndAt}",
            workOrder.Id,
            request.Spot,
            request.NewStartAt,
            newEndAt);

        await cache.RemoveByTagAsync(CacheTags.WorkOrderById(request.WorkOrderId), cancellationToken);

        return Result.Updated;
    }
}
