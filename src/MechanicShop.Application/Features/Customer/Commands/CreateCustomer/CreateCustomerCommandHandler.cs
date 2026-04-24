using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Customer.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler(
    IAppDbContext context,
    ILogger<CreateCustomerCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLower().Trim();
        if (await context.Customers.AnyAsync(c => c.Email.ToLower().Trim() == email, cancellationToken))
        {
            logger.LogWarning("A customer with the same email already exists");
            return ApplicationErrors.CustomerEmailAlreadyExists;
        }

        var vehicles = new List<Vehicle>();
        foreach (var vehicle in request.Vehicles)
        {
            var vehicleResult = Vehicle.Create(
                vehicle.Make,
                vehicle.Model,
                vehicle.Year,
                vehicle.LicensePlate
            );
            if (!vehicleResult.IsSuccess)
            {
                logger.LogWarning("Failed to create vehicle for customer: {Error}", vehicleResult.TopError);
                return vehicleResult.Errors.ToList();
            }
            vehicles.Add(vehicleResult.Value);
        }
        var createCustomerResult = Domain.Customers.Customer.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            vehicles);

        if (!createCustomerResult.IsSuccess)
        {
            logger.LogWarning("Failed to create customer: {Error}", createCustomerResult.TopError);
            return createCustomerResult.Errors.ToList();
        }

        var customer = createCustomerResult.Value;

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

        await cache.RemoveByTagAsync(CacheTags.Customer, cancellationToken);

        return customer.ToDto();
    }
}