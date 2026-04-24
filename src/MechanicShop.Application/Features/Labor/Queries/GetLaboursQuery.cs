using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labor.Dtos;
namespace MechanicShop.Application.Features.Labor.Queries;

public sealed class GetLaborsQuery : ICachedQuery<List<LaborDto>>
{
    public string CacheKey => CacheKeys.LaborList();

    public string[] CacheTag => [CacheTags.Labors];

    public TimeSpan CacheDuration => CacheDurations.LaborList;
}