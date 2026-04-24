using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customer.Dtos;
namespace MechanicShop.Application.Features.Customer.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid Id) : ICachedQuery<CustomerDto>
{
    public string CacheKey => CacheKeys.CustomerById(Id);

    public string[] CacheTag => [CacheTags.Customer];

    public TimeSpan CacheDuration => CacheDurations.CustomerById;
}