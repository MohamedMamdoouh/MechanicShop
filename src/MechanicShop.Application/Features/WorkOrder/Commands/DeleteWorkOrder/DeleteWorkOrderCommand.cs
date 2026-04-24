using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.WorkOrder.Commands.DeleteWorkOrder;

public sealed record DeleteWorkOrderCommand(Guid WorkOrderId) : IRequest<Result<Deleted>>;