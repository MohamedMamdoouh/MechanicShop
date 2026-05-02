using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Commands.Login;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Identity;
using MechanicShop.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
namespace MechanicShop.Application.SubcutaneousTests.Features.Identity;

public abstract class IdentityTestBase(WebAppFactory factory)
{
    protected IMediator Mediator { get; } = factory.CreateMediator();

    protected async Task<(string Email, string Password)> SeedUserAsync()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        const string roleName = nameof(Role.Manager);
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));

        var id = Guid.NewGuid().ToString("N")[..8];
        var email = $"test-{id}@example.com";
        const string password = "Test@1234!";

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, roleName);

        return (email, password);
    }

    protected async Task<TokenResponse> LoginAsync(string email, string password)
    {
        var command = new LoginCommand(email, password, "test-device");
        var result = await Mediator.Send(command);
        return result.Value;
    }
}
