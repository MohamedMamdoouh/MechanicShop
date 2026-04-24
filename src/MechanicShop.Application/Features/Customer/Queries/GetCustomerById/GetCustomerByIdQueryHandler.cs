using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Customer.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(
    ILogger<GetCustomerByIdQueryHandler> logger,
    IAppDbContext context)
        : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.AsNoTracking()
            .Include(v => v.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            logger.LogWarning("Customer with ID {CustomerId} not found.", request.Id);
            return ApplicationErrors.CustomerNotFound;
        }

        return customer.ToDto();
    }
}