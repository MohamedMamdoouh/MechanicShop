using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enum;
using MediatR;
namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed record CreateRepairTaskCommand(
    string Name,
    decimal LaborCost,
    RepairDurationMinutes RepairDurationMinutes,
    List<CreateRepairTaskPartCommand> Parts
) : IRequest<Result<RepairTaskDto>>;