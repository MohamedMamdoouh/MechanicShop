using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.RepairTasks;
namespace MechanicShop.Tests.Common.WorkOrders;

public static class WorkOrderFactory
{
    public static Result<WorkOrder> Create(
        Guid? vehicleId = null,
        DateTimeOffset? startAtUtc = null,
        DateTimeOffset? endAtUtc = null,
        Guid? laborId = null,
        Spot? spot = null,
        List<RepairTask>? repairTasks = null,
        decimal? discount = null,
        decimal? taxPercentage = null)
    {
        return WorkOrder.Create(
            vehicleId ?? Guid.NewGuid(),
            startAtUtc ?? DateTimeOffset.UtcNow,
            endAtUtc ?? DateTimeOffset.UtcNow.AddHours(1),
            laborId ?? Guid.NewGuid(),
            spot ?? Spot.A,
            repairTasks ?? [RepairTaskFactory.Create().Value],
            discount ?? 0m,
            taxPercentage ?? 0m);
    }
}