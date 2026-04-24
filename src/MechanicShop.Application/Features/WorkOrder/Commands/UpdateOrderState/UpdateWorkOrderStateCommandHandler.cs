using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Commands.UpdateOrderState;

public sealed class UpdateWorkOrderStateCommandHandler(
    IAppDbContext context,
    ILogger<UpdateWorkOrderStateCommandHandler> logger,
    HybridCache cache,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        UpdateWorkOrderStateCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders.FirstOrDefaultAsync(
            x => x.Id == request.WorkOrderId,
            cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("Work order with id {WorkOrderId} not found", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        var updateWorkOrderStateResult = workOrder.UpdateStatus(request.NewState, timeProvider.GetUtcNow());

        if (!updateWorkOrderStateResult.IsSuccess)
        {
            logger.LogWarning("Failed to update work order state for work order with id {WorkOrderId}. Reason: {Reason}",
                request.WorkOrderId, updateWorkOrderStateResult.Errors);

            return updateWorkOrderStateResult.Errors.ToList();
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified(workOrder.Id));

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(CacheTags.WorkOrderById(request.WorkOrderId), cancellationToken);

        return Result.Updated;
    }
}