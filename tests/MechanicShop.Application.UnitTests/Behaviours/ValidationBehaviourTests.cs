using FluentValidation;
using FluentValidation.Results;
using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Domain.Common.Results;
using NSubstitute;
using Xunit;
namespace MechanicShop.Application.UnitTests.Behaviours;

public class ValidationBehaviourTests
{
    // Convenience: a next delegate that simply returns the given value.
    private static MediatR.RequestHandlerDelegate<T> Next<T>(T value)
        => _ => Task.FromResult(value);

    [Fact]
    public async Task Handle_WhenNoValidatorRegistered_CallsNextAndReturnsResponse()
    {
        var behaviour = new ValidationBehaviour<FakeRequest, Result<string>>(validator: null);

        var result = await behaviour.Handle(new FakeRequest(), Next<Result<string>>("ok"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNextAndReturnsResponse()
    {
        var validator = Substitute.For<IValidator<FakeRequest>>();

        validator.ValidateAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behaviour = new ValidationBehaviour<FakeRequest, Result<string>>(validator);

        var result = await behaviour.Handle(new FakeRequest(), Next<Result<string>>("ok"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await validator.Received(1).ValidateAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsFailedResult()
    {
        var validator = Substitute.For<IValidator<FakeRequest>>();

        validator.ValidateAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Required") { ErrorCode = "NotEmpty" }]));

        var behaviour = new ValidationBehaviour<FakeRequest, Result<string>>(validator);

        var result = await behaviour.Handle(new FakeRequest(), Next<Result<string>>("ok"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Description == "Required" && e.Code == "NotEmpty");
    }

    [Fact]
    public async Task Handle_WhenValidationFails_NextIsNeverCalled()
    {
        var validator = Substitute.For<IValidator<FakeRequest>>();

        validator.ValidateAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Required") { ErrorCode = "NotEmpty" }]));

        var behaviour = new ValidationBehaviour<FakeRequest, Result<string>>(validator);
        var nextCalled = false;

        await behaviour.Handle(
            new FakeRequest(),
            _ => { nextCalled = true; return Task.FromResult<Result<string>>("ok"); },
            CancellationToken.None);

        Assert.False(nextCalled);
    }
}
