using MechanicShop.Domain.RepairTasks.Enum;
namespace MechanicShop.Contracts.RepairTasks;

public sealed record CreateRepairTaskRequest(
    string Name,
    decimal LaborCost,
    RepairDurationMinutes RepairDurationMinutes,
    List<CreateRepairTaskPartRequest> Parts);

public sealed record CreateRepairTaskPartRequest(
    string Name,
    decimal Cost,
    int Quantity);

public sealed record UpdateRepairTaskRequest(
    Guid RepairTaskId,
    string Name,
    decimal LaborCost,
    RepairDurationMinutes RepairDurationMinutes,
    List<UpdateRepairTaskPartRequest> Parts);

public sealed record UpdateRepairTaskPartRequest(
    Guid PartId,
    string Name,
    decimal Cost,
    int Quantity);
