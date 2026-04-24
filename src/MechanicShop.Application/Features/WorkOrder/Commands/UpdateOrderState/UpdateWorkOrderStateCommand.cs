using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enum;
using MediatR;
namespace MechanicShop.Application.Features.WorkOrder.Commands.UpdateOrderState;

public sealed record UpdateWorkOrderStateCommand(
    Guid WorkOrderId,
    WorkOrderState NewState) : IRequest<Result<Updated>>;