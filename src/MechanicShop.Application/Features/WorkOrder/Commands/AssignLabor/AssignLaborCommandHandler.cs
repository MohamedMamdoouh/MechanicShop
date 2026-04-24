using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Commands.AssignLabor;

public sealed class AssignLaborCommandHandler(
    IAppDbContext context,
    ILogger<AssignLaborCommandHandler> logger,
    HybridCache cache,
    IWorkOrderService workOrderService)
    : IRequestHandler<AssignLaborCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AssignLaborCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders
        .Include(x => x.Vehicle).ThenInclude(x => x.Customer)
        .FirstOrDefaultAsync(x => x.Id == request.WorkOrderId, cancellationToken);

        if (workOrder == null)
        {
            logger.LogWarning("Work order with ID {WorkOrderId} not found", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        var labor = await context.Employees.FindAsync([request.LaborId], cancellationToken);

        if (labor == null)
        {
            logger.LogWarning("Labor with ID {LaborId} not found", request.LaborId);
            return ApplicationErrors.LaborNotFound;
        }

        if (await workOrderService.IsLaborOccupiedAsync(
            request.LaborId,
            workOrder.StartAtUtc,
            workOrder.EndAtUtc,
            null,
            cancellationToken))
        {
            logger.LogWarning("Labor with ID {LaborId} is already assigned to another work order during the same time period", request.LaborId);
            return ApplicationErrors.LaborOccupied;
        }

        var updatedLaborResult = workOrder.UpdateLabor(request.LaborId);

        if (!updatedLaborResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to assign labor with ID {LaborId} to work order with ID {WorkOrderId}: {ErrorMessage}",
                request.LaborId,
                request.WorkOrderId,
                updatedLaborResult.TopError);

            return updatedLaborResult.Errors.ToList();
        }

        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.WorkOrderById(request.WorkOrderId), cancellationToken);
        return Result.Updated;
    }
}