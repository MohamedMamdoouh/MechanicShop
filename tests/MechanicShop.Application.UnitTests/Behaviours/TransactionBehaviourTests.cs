using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
namespace MechanicShop.Application.UnitTests.Behaviours;

public class TransactionBehaviourTests
{
    private static TransactionBehaviour<TReq, Result<string>> CreateBehaviour<TReq>(
        ILogger<TransactionBehaviour<TReq, Result<string>>>? logger = null,
        IAppDbContext? context = null)
        where TReq : notnull
    {
        logger ??= Substitute.For<ILogger<TransactionBehaviour<TReq, Result<string>>>>();
        context ??= Substitute.For<IAppDbContext>();
        return new TransactionBehaviour<TReq, Result<string>>(logger, context);
    }

    [Fact]
    public async Task Handle_WhenRequestIsCachedQuery_SkipsTransactionAndCallsNextDirectly()
    {
        // FakeCachedRequest implements ICachedQuery, so TransactionBehaviour skips the
        // database transaction and calls next() immediately.
        var context = Substitute.For<IAppDbContext>();
        var behaviour = CreateBehaviour<FakeCachedRequest>(context: context);
        var nextCalled = false;

        var result = await behaviour.Handle(
            new FakeCachedRequest(),
            _ => { nextCalled = true; return Task.FromResult<Result<string>>("ok"); },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);

        // Database must never be touched for cache-only reads.
        _ = context.DidNotReceive().Database;
    }
}
