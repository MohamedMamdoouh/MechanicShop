using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Domain.Common.Results;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;
namespace MechanicShop.Application.UnitTests.Behaviours;

public class CachingBehaviourTests
{
    private static CachingBehaviour<TReq, Result<string>> CreateBehaviour<TReq>(
        HybridCache? cache = null,
        ILogger<CachingBehaviour<TReq, Result<string>>>? logger = null)
        where TReq : notnull
    {
        cache ??= Substitute.For<HybridCache>();
        logger ??= Substitute.For<ILogger<CachingBehaviour<TReq, Result<string>>>>();
        return new CachingBehaviour<TReq, Result<string>>(cache, logger);
    }

    [Fact]
    public async Task Handle_WhenRequestIsNotCachedQuery_CallsNextDirectly()
    {
        // FakeRequest does not implement ICachedQuery, so the cache is bypassed.
        var cache = Substitute.For<HybridCache>();
        var behaviour = CreateBehaviour<FakeRequest>(cache: cache);
        var nextCalled = false;

        var result = await behaviour.Handle(
            new FakeRequest(),
            _ => { nextCalled = true; return Task.FromResult<Result<string>>("ok"); },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_WhenRequestIsNotCachedQuery_NeverAccessesCache()
    {
        var cache = Substitute.For<HybridCache>();
        var behaviour = CreateBehaviour<FakeRequest>(cache: cache);

        await behaviour.Handle(
            new FakeRequest(),
            _ => Task.FromResult<Result<string>>("ok"),
            CancellationToken.None);

        // HybridCache.GetOrCreateAsync must not be called for non-cached requests.
        await cache.DidNotReceive().GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
            Arg.Any<HybridCacheEntryOptions?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRequestIsCachedQuery_UsesCache()
    {
        var cache = Substitute.For<HybridCache>();
        Result<string> expected = "cached-value";

        _ = cache.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
                Arg.Any<HybridCacheEntryOptions?>(),
                Arg.Any<IEnumerable<string>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<Result<string>>(expected));

        var behaviour = CreateBehaviour<FakeCachedRequest>(cache: cache);

        var result = await behaviour.Handle(
            new FakeCachedRequest(),
            _ => Task.FromResult<Result<string>>("should-not-be-called"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("cached-value", result.Value);
    }
}
