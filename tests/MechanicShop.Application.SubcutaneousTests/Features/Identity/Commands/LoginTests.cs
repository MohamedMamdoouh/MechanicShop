using MechanicShop.Application.Features.Identity.Commands.Login;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LoginTests(WebAppFactory factory) : IdentityTestBase(factory)
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokenResponse()
    {
        var (email, password) = await SeedUserAsync();

        var command = new LoginCommand(email, password, "device-001");

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.AccessToken);
        Assert.NotNull(result.Value.RefreshToken);
        Assert.True(result.Value.AccessTokenExpiresOnUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsAuthenticationError()
    {
        var (email, _) = await SeedUserAsync();

        var command = new LoginCommand(email, "WrongPassword!", "device-001");

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Authentication.AuthenticationFailed", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ReturnsAuthenticationError()
    {
        var command = new LoginCommand("nobody@example.com", "SomePassword!", "device-001");

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Authentication.AuthenticationFailed", result.TopError!.Value.Code);
    }
}
