using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enum;
using MediatR;
namespace MechanicShop.Application.Features.WorkOrder.Commands.RelocateWorkOrder;

public sealed record RelocateWorkOrderCommand(
    Guid WorkOrderId,
    DateTimeOffset NewStartAt,
    Spot Spot) : IRequest<Result<Updated>>;