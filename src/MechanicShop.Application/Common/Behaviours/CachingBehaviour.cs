using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Common.Behaviours;

public class CachingBehaviour<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(cancellationToken);
        }

        logger.LogInformation("Fetching from cache with key: {CacheKey}", cachedRequest.CacheKey);

        try
        {
            return await cache.GetOrCreateAsync(
                cachedRequest.CacheKey,
                async ct =>
                {
                    logger.LogInformation("Cache miss for key: {CacheKey}. Executing handler.", cachedRequest.CacheKey);

                    var response = await next(ct);

                    if (response is IResult { IsSuccess: false })
                    {
                        logger.LogInformation("Skipping cache for failed result with key: {CacheKey}", cachedRequest.CacheKey);
                        throw new SkipCacheException(response);
                    }

                    return response;
                },
                new HybridCacheEntryOptions { Expiration = cachedRequest.CacheDuration },
                tags: cachedRequest.CacheTag,
                cancellationToken: cancellationToken);
        }
        catch (SkipCacheException ex)
        {
            return ex.Response;
        }
    }

    private sealed class SkipCacheException(TResponse response) : Exception
    {
        public TResponse Response { get; } = response;
    }
}