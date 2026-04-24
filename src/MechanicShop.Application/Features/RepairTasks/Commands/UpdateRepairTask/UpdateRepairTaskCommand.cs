using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enum;
using MediatR;
namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed record UpdateRepairTaskCommand(
    Guid RepairTaskId,
    string Name,
    decimal LaborCost,
    RepairDurationMinutes RepairDurationMinutes,
    List<UpdateRepairTaskPartCommand> Parts)
    : IRequest<Result<Updated>>;