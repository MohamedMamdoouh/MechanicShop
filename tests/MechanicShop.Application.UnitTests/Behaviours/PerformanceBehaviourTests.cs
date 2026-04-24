using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
namespace MechanicShop.Application.UnitTests.Behaviours;

public class PerformanceBehaviourTests
{
    private static PerformanceBehaviour<FakeRequest, Result<string>> CreateBehaviour(
        int thresholdMs,
        ILogger<PerformanceBehaviour<FakeRequest, Result<string>>>? logger = null)
    {
        logger ??= Substitute.For<ILogger<PerformanceBehaviour<FakeRequest, Result<string>>>>();
        var options = Options.Create(new PerformanceSettings { LongRunningRequestThresholdInMs = thresholdMs });
        return new PerformanceBehaviour<FakeRequest, Result<string>>(logger, options);
    }

    [Fact]
    public async Task Handle_AlwaysCallsNextAndReturnsResponse()
    {
        var behaviour = CreateBehaviour(thresholdMs: 10_000);

        var result = await behaviour.Handle(
            new FakeRequest(),
            _ => Task.FromResult<Result<string>>("ok"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_WhenRequestExceedsThreshold_LogsWarning()
    {
        var logger = Substitute.For<ILogger<PerformanceBehaviour<FakeRequest, Result<string>>>>();

        // Threshold of 1ms; the async next adds a small delay to ensure elapsed > 1ms.
        var behaviour = CreateBehaviour(thresholdMs: 1, logger: logger);

        await behaviour.Handle(
            new FakeRequest(),
            async ct => { await Task.Delay(10, ct); return (Result<string>)"ok"; },
            CancellationToken.None);

        logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenRequestBelowThreshold_DoesNotLogWarning()
    {
        var logger = Substitute.For<ILogger<PerformanceBehaviour<FakeRequest, Result<string>>>>();
        var behaviour = CreateBehaviour(thresholdMs: 10_000, logger: logger);

        await behaviour.Handle(
            new FakeRequest(),
            _ => Task.FromResult<Result<string>>("ok"),
            CancellationToken.None);

        logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenRequestThrowsAndExceedsThreshold_StillLogsWarning()
    {
        // Slow failure (e.g. DB timeout) must still trigger the long-running warning.
        var logger = Substitute.For<ILogger<PerformanceBehaviour<FakeRequest, Result<string>>>>();
        var behaviour = CreateBehaviour(thresholdMs: 1, logger: logger);

        await Assert.ThrowsAsync<TimeoutException>(() => behaviour.Handle(
            new FakeRequest(),
            async ct => { await Task.Delay(10, ct); throw new TimeoutException("DB timeout"); },
            CancellationToken.None));

        logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
