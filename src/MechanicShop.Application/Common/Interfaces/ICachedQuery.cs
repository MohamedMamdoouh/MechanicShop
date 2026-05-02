using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Common.Interfaces;

public interface ICachedQuery
{
    string CacheKey { get; }
    string[] CacheTag { get; }
    TimeSpan CacheDuration { get; }
}

public interface ICachedQuery<TValue> : IRequest<Result<TValue>>, ICachedQuery
{
}