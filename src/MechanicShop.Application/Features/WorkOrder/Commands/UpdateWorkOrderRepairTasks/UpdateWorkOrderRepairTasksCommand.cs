using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.WorkOrder.Commands.UpdateWorkOrderRepairTasks;

public sealed record UpdateWorkOrderRepairTasksCommand(
    Guid WorkOrderId,
    List<Guid> RepairTaskIds) : IRequest<Result<Updated>>;