using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customer.Dtos;
namespace MechanicShop.Application.Features.Customer.Queries.GetCustomers;

public sealed record GetCustomersQuery(int PageNumber = 1, int PageSize = 10)
: ICachedQuery<PaginatedList<CustomerDto>>
{
    public string CacheKey => CacheKeys.CustomerListPaginated(PageNumber, PageSize);

    public string[] CacheTag => [CacheTags.Customer];

    public TimeSpan CacheDuration => CacheDurations.CustomerList;
}