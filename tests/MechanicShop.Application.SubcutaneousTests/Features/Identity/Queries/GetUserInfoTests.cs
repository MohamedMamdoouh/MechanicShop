using MechanicShop.Application.Features.Identity.Queries;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetUserInfoTests(WebAppFactory factory) : IdentityTestBase(factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsUserInfo()
    {
        var (email, _) = await SeedUserAsync();

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);

        var query = new GetUserInfoQuery(user!.Id);

        var result = await Mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value.Email);
        Assert.NotEmpty(result.Value.Roles);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        var query = new GetUserInfoQuery(Guid.NewGuid().ToString());

        var result = await Mediator.Send(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Authentication.UserNotFound", result.TopError!.Value.Code);
    }
}
