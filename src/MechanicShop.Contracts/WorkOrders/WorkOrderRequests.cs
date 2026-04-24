using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Contracts.WorkOrders;

public sealed record AssignLaborRequest(Guid LaborId);

public sealed record RelocateWorkOrderRequest(
    DateTimeOffset NewStartAt,
    Spot Spot);

public sealed record UpdateWorkOrderStateRequest(WorkOrderState NewState);

public sealed record UpdateWorkOrderRepairTasksRequest(List<Guid> RepairTaskIds);

public sealed record CreateWorkOrderRequest(
    Spot Spot,
    Guid VehicleId,
    DateTimeOffset StartAt,
    List<Guid> RepairTaskIds,
    Guid LaborId);
