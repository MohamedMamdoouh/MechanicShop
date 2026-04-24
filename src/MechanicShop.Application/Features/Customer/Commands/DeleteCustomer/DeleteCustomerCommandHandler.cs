using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Customer.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandHandler(
    IAppDbContext context,
    ILogger<DeleteCustomerCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<DeleteCustomerCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer is null)
        {
            logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        var hasActiveWorkOrders = await context.WorkOrders
            .AsNoTracking()
            .AnyAsync(
                wo => wo.Vehicle.CustomerId == request.CustomerId
                      && (wo.Status == WorkOrderState.Scheduled || wo.Status == WorkOrderState.InProgress),
                cancellationToken);

        if (hasActiveWorkOrders)
        {
            logger.LogWarning("Cannot delete customer {CustomerId} — has active work orders", request.CustomerId);
            return ApplicationErrors.CustomerHasActiveWorkOrders;
        }

        context.Customers.Remove(customer);
        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.Customer, cancellationToken);
        logger.LogInformation("Customer {CustomerId} deleted successfully", customer.Id);

        return Result.Deleted;
    }
}
