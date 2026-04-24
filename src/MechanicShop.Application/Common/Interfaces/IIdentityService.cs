using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken);

    Task<string?> GetUsernameAsync(string userId);
}