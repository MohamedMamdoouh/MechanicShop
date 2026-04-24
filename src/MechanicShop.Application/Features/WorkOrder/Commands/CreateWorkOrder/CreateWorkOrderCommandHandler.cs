using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.Features.WorkOrder.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler(
    IAppDbContext context,
    ILogger<CreateWorkOrderCommandHandler> logger,
    HybridCache cache,
    IWorkOrderService workOrderService)
    : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
    public async Task<Result<WorkOrderDto>> Handle(
        CreateWorkOrderCommand request,
        CancellationToken cancellationToken)
    {
        var repairTasks = await context.RepairTasks
        .Where(rt => request.RepairTaskIds.Contains(rt.Id))
        .ToListAsync(cancellationToken);

        if (repairTasks.Count != request.RepairTaskIds.Count)
        {
            var missingIds = request.RepairTaskIds.Except(repairTasks.Select(rt => rt.Id));
            logger.LogWarning("Some repair tasks were not found: {MissingIds}", string.Join(", ", missingIds));
            return ApplicationErrors.RepairTaskNotFound;
        }

        var endAt = workOrderService.CalculateEndTime(request.StartAt, repairTasks);

        if (workOrderService.IsOutsideOperatingHours(request.StartAt, endAt))
        {
            logger.LogWarning(
                 "Work order time range is outside of operating hours: {StartAt} - {EndAt}",
                 request.StartAt,
                 endAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHours;
        }

        var checkRequirementsResult = workOrderService.ValidateMinimumRequiredTime(request.StartAt, endAt);

        if (!checkRequirementsResult.IsSuccess)
        {
            logger.LogWarning(
                "Work order does not meet minimum required time: {StartAt} - {EndAt}",
                request.StartAt,
                endAt);

            return checkRequirementsResult.Errors.ToList();
        }

        var isSpotAvailable = await workOrderService.IsSpotAvailableAsync(
            request.Spot,
            request.StartAt,
            endAt,
            cancellationToken);

        if (!isSpotAvailable)
        {
            logger.LogWarning(
                "Work order spot is not available: {Spot} at {StartAt} - {EndAt}",
                request.Spot,
                request.StartAt,
                endAt);

            return ApplicationErrors.WorkOrderSpotNotAvailable;
        }

        var vehicle = await context.Vehicles.Include(v => v.Customer)
        .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);

        if (vehicle == null)
        {
            logger.LogWarning("Vehicle not found: {VehicleId}", request.VehicleId);
            return ApplicationErrors.VehicleNotFound;
        }

        var labor = await context.Employees.FindAsync([request.LaborId], cancellationToken);

        if (labor == null)
        {
            logger.LogWarning("Labor not found: {LaborId}", request.LaborId);
            return ApplicationErrors.LaborNotFound;
        }

        var vehicleConflictResult = await workOrderService.CheckVehicleConflictsAsync(
            request.VehicleId,
            request.StartAt,
            endAt,
            cancellationToken);

        if (!vehicleConflictResult.IsSuccess)
        {
            logger.LogWarning(
                "Vehicle has conflicting work orders: {VehicleId} at {StartAt} - {EndAt}",
                request.VehicleId,
                request.StartAt,
                endAt);

            return vehicleConflictResult.Errors.ToList();
        }

        var isLaborOccupied = await workOrderService.IsLaborOccupiedAsync(
            request.LaborId,
            request.StartAt,
            endAt,
            null,
            cancellationToken);

        if (isLaborOccupied)
        {
            logger.LogWarning(
                "Labor is occupied: {LaborId} at {StartAt} - {EndAt}",
                request.LaborId,
                request.StartAt,
                endAt);

            return ApplicationErrors.LaborOccupied;
        }

        var workOrderCreateResult = Domain.WorkOrders.WorkOrder.Create(
        request.VehicleId,
        request.StartAt,
        endAt,
        request.LaborId,
        request.Spot,
        repairTasks);

        if (!workOrderCreateResult.IsSuccess)
        {
            logger.LogWarning(
                "Work order creation failed due to validation errors: {Errors}",
                workOrderCreateResult.Errors);

            return workOrderCreateResult.Errors.ToList();
        }

        var workOrder = workOrderCreateResult.Value;

        workOrder.AddDomainEvent(new WorkOrderCollectionModified(workOrder.Id));

        context.WorkOrders.Add(workOrder);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work order created successfully: {WorkOrderId}", workOrder.Id);

        await cache.RemoveByTagAsync(CacheTags.WorkOrders, cancellationToken);

        return workOrder.ToDto();
    }
}