using System.Diagnostics;
using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
namespace MechanicShop.Application.UnitTests.Behaviours;

public class LoggingBehaviourTests
{
    private readonly ILogger<LoggingBehavior<FakeRequest, Result<string>>> _logger
        = Substitute.For<ILogger<LoggingBehavior<FakeRequest, Result<string>>>>();

    private readonly IUser _user = Substitute.For<IUser>();

    private LoggingBehavior<FakeRequest, Result<string>> CreateBehaviour()
        => new(_logger, _user);

    [Fact]
    public async Task Handle_OnSuccess_ReturnsResponseFromNext()
    {
        _user.Id.Returns("user-123");
        var behaviour = CreateBehaviour();

        var result = await behaviour.Handle(
            new FakeRequest(),
            _ => Task.FromResult<Result<string>>("ok"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_OnSuccess_LogsRequestNameAndUserId()
    {
        _user.Id.Returns("user-123");
        var behaviour = CreateBehaviour();

        await behaviour.Handle(
            new FakeRequest(),
            _ => Task.FromResult<Result<string>>("ok"),
            CancellationToken.None);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(static o => o.ToString()!.Contains("FakeRequest") && o.ToString()!.Contains("user-123")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_OnSuccess_LogsTraceId()
    {
        // Start a real Activity so Activity.Current is populated.
        using var activity = new Activity("Test").Start();
        var traceId = activity.TraceId.ToString();
        _user.Id.Returns("user-123");
        var behaviour = CreateBehaviour();

        await behaviour.Handle(
            new FakeRequest(),
            _ => Task.FromResult<Result<string>>("ok"),
            CancellationToken.None);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(traceId)),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenNextThrows_LogsErrorWithContentAndRethrows()
    {
        using var activity = new Activity("Test").Start();
        var traceId = activity.TraceId.ToString();
        _user.Id.Returns("user-123");
        var behaviour = CreateBehaviour();
        var boom = new InvalidOperationException("boom");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behaviour.Handle(new FakeRequest(), _ => throw boom, CancellationToken.None));

        Assert.Same(boom, ex);

        // Error log must include request name, elapsed time, and trace id.
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("FakeRequest")
                             && o.ToString()!.Contains("ms")
                             && o.ToString()!.Contains(traceId)),
            boom,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
