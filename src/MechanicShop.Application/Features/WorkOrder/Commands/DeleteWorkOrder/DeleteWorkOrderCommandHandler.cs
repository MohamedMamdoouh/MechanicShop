using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandHandler(
    IAppDbContext context,
    IWorkOrderService workOrderService,
    ILogger<DeleteWorkOrderCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

        if (workOrder == null)
        {
            logger.LogWarning("Work order with ID {WorkOrderId} not found.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        var canDeleteWorkOrder = workOrderService.CanDeleteWorkOrder(workOrder);

        if (!canDeleteWorkOrder)
        {
            logger.LogWarning(
                "Work order with ID {WorkOrderId} cannot be deleted due to its current status: {Status}.",
                request.WorkOrderId,
                workOrder.Status);

            return ApplicationErrors.ReadonlyWorkOrder;
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified(workOrder.Id));

        context.WorkOrders.Remove(workOrder);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Work order with ID {WorkOrderId} has been deleted.", request.WorkOrderId);

        await cache.RemoveByTagAsync(CacheTags.WorkOrderById(request.WorkOrderId), cancellationToken);

        return Result.Deleted;
    }
}