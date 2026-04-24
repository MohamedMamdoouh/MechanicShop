using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Customer.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(
    IAppDbContext context,
    ILogger<UpdateCustomerCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer is null)
        {
            logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        var email = request.Email.ToLower().Trim();
        if (await context.Customers.AnyAsync(
                c => c.Id != request.CustomerId && c.Email.ToLower().Trim() == email,
                cancellationToken))
        {
            logger.LogWarning("A customer with the same email already exists");
            return CustomerErrors.EmailAlreadyExists;
        }

        var updateResult = customer.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);
        if (!updateResult.IsSuccess)
        {
            logger.LogWarning("Failed to update customer {CustomerId}: {Error}", request.CustomerId, updateResult.TopError);
            return updateResult.Errors.ToList();
        }

        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.Customer, cancellationToken);
        logger.LogInformation("Customer {CustomerId} updated successfully", customer.Id);

        return Result.Updated;
    }
}
