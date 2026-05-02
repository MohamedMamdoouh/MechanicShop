using MechanicShop.Application.Features.Identity.Commands.Logout;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LogoutTests(WebAppFactory factory) : IdentityTestBase(factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Handle_AfterLogin_RevokesSessionSuccessfully()
    {
        var (email, _) = await SeedUserAsync();

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);

        var command = new LogoutCommand(user!.Id, "test-device");

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
    }
}
