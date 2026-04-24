using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace MechanicShop.Infrastructure.Services;

public sealed class WorkOrderService(
    IAppDbContext context,
    IOptions<AppSettings> appSettings) : IWorkOrderService
{
    public DateTimeOffset CalculateEndTime(DateTimeOffset startTime, IEnumerable<RepairTask> repairTasks)
    {
        ArgumentNullException.ThrowIfNull(repairTasks);

        var totalMinutes = repairTasks.Sum(rt => (int)rt.EstimatedRepairDurationMinutes);
        return startTime.AddMinutes(totalMinutes);
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            return true;
        }

        var tz = TimeZoneInfo.FindSystemTimeZoneById(appSettings.Value.ShopTimeZone);
        var localStart = TimeZoneInfo.ConvertTime(startTime, tz);
        var localEnd = TimeZoneInfo.ConvertTime(endTime, tz);

        // Work orders are expected to fit in a single business day window.
        if (localStart.Date != localEnd.Date)
        {
            return true;
        }

        var opening = appSettings.Value.OpeningTime;
        var closing = appSettings.Value.ClosingTime;

        var start = TimeOnly.FromDateTime(localStart.DateTime);
        var end = TimeOnly.FromDateTime(localEnd.DateTime);

        return start < opening || end > closing;
    }

    public async Task<bool> IsLaborOccupiedAsync(
        Guid laborId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Guid? excludeWorkOrderId = null,
        CancellationToken cancellationToken = default)
    {
        return await context.WorkOrders
            .AsNoTracking()
            .Where(wo => wo.LaborId == laborId)
            .Where(wo => wo.Status != WorkOrderState.Cancelled)
            .Where(wo => !excludeWorkOrderId.HasValue || wo.Id != excludeWorkOrderId.Value)
            .AnyAsync(wo => wo.StartAtUtc < endTime && wo.EndAtUtc > startTime, cancellationToken);
    }

    public async Task<bool> IsVehicleAlreadyScheduledAsync(
        Guid vehicleId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Guid? excludeWorkOrderId = null,
        CancellationToken cancellationToken = default)
    {
        return await context.WorkOrders
            .AsNoTracking()
            .Where(wo => wo.VehicleId == vehicleId)
            .Where(wo => wo.Status != WorkOrderState.Cancelled)
            .Where(wo => !excludeWorkOrderId.HasValue || wo.Id != excludeWorkOrderId.Value)
            .AnyAsync(wo => wo.StartAtUtc < endTime && wo.EndAtUtc > startTime, cancellationToken);
    }

    public async Task<bool> IsSpotAvailableAsync(
        Spot spot,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        var hasOverlappingSpot = await context.WorkOrders
            .AsNoTracking()
            .Where(wo => wo.Spot == spot)
            .Where(wo => wo.Status != WorkOrderState.Cancelled)
            .AnyAsync(wo => wo.StartAtUtc < endTime && wo.EndAtUtc > startTime, cancellationToken);

        return !hasOverlappingSpot;
    }

    private static bool ValidateMinimumLaborHours(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var durationInMinutes = (endTime - startTime).TotalMinutes;
        return durationInMinutes >= 15;
    }

    public Result<Success> ValidateMinimumRequiredTime(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (endAt <= startAt)
        {
            return Error.Validation(
                "The work order end time must be after start time.",
                "ApplicationErrors.WorkOrder.InvalidTimeRange");
        }

        if (!ValidateMinimumLaborHours(startAt, endAt))
        {
            return Error.Validation(
                "The minimum work order duration is 15 minutes.",
                "ApplicationErrors.WorkOrder.MinimumDuration");
        }

        var durationInMinutes = (endAt - startAt).TotalMinutes;

        if (durationInMinutes > appSettings.Value.MaxAppointmentDurationInMinutes)
        {
            return Error.Validation(
                "The work order duration exceeds maximum allowed appointment duration.",
                "ApplicationErrors.WorkOrder.MaxDurationExceeded");
        }

        return Result.Success;
    }

    public async Task<Result<Success>> CheckVehicleConflictsAsync(
        Guid vehicleId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken cancellationToken = default)
    {
        var hasConflict = await IsVehicleAlreadyScheduledAsync(vehicleId, startAt, endAt, null, cancellationToken);
        return hasConflict ? ApplicationErrors.VehicleSchedulingConflict : Result.Success;
    }

    public bool CanDeleteWorkOrder(Domain.WorkOrders.WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);
        return workOrder.Status == WorkOrderState.Scheduled;
    }
}
