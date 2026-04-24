using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Application.Features.Customer.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler(IAppDbContext context)
    : IRequestHandler<GetCustomersQuery, Result<PaginatedList<CustomerDto>>>
{
    public async Task<Result<PaginatedList<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customersQuery = context.Customers
            .Include(c => c.Vehicles)
            .AsNoTracking()
            .OrderBy(c => c.CreatedAtUtc)
            .AsQueryable();

        var totalCount = await customersQuery.CountAsync(cancellationToken);

        var customers = await customersQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<CustomerDto>
        {
            Items = customers.ToDtoList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}