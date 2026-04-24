using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.UnitTests.Behaviours;

public record FakeRequest : IRequest<Result<string>>;

public record FakeCachedRequest : IRequest<Result<string>>, ICachedQuery<string>
{
    public string CacheKey => "test-cache-key";
    public string[] CacheTag => ["test-tag"];
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
