using MediatR;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Application.Common.Interfaces;

public interface ICachedQuery
{
    public string CacheKey { get; }
    public string[] CacheTag { get; }
    public TimeSpan CacheDuration { get; }
}

public interface ICachedQuery<TValue> : IRequest<Result<TValue>>, ICachedQuery
{
}