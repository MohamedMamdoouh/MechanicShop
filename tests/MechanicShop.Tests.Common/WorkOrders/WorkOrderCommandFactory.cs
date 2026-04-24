using MechanicShop.Application.Features.WorkOrder.Commands.CreateWorkOrder;
using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Tests.Common.WorkOrders;

public static class WorkOrderCommandFactory
{
    public static CreateWorkOrderCommand Create(
        Spot? spot = null,
        Guid? vehicleId = null,
        DateTimeOffset? startAt = null,
        List<Guid>? repairTaskIds = null,
        Guid? laborId = null)
    {
        return new CreateWorkOrderCommand(spot ?? Spot.A, vehicleId ?? Guid.NewGuid(), startAt
            ?? DateTimeOffset.UtcNow, repairTaskIds ?? [Guid.NewGuid()], laborId ?? Guid.NewGuid());
    }
}