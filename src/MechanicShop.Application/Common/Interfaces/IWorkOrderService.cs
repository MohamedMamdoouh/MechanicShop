using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderService
{
    DateTimeOffset CalculateEndTime(
        DateTimeOffset startTime,
        IEnumerable<RepairTask> repairTasks);

    bool IsOutsideOperatingHours(
        DateTimeOffset startTime,
        DateTimeOffset endTime);

    Task<bool> IsLaborOccupiedAsync(
        Guid laborId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Guid? excludeWorkOrderId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsVehicleAlreadyScheduledAsync(
        Guid vehicleId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Guid? excludeWorkOrderId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsSpotAvailableAsync(
        Spot spot,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    Result<Success> ValidateMinimumRequiredTime(
        DateTimeOffset startAt,
        DateTimeOffset endAt);

    Task<Result<Success>> CheckVehicleConflictsAsync(
        Guid vehicleId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken cancellationToken = default);

    bool CanDeleteWorkOrder(Domain.WorkOrders.WorkOrder workOrder);
}
