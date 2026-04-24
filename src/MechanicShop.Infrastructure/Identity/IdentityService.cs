using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    IMemoryCache cache,
    ILogger<IdentityService> logger)
    : IIdentityService
{

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            logger.LogWarning(
                "Failed authentication attempt for email: {Email}",
                UtilityService.MaskEmail(email));

            return Error.Conflict(
                $"Invalid email or password for email: {UtilityService.MaskEmail(email)}");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning(
                "Email not confirmed for email: {Email}",
                UtilityService.MaskEmail(email));

            return Error.Conflict(
                $"Email '{UtilityService.MaskEmail(email)}' not confirmed");
        }

        return new AppUserDto(
            user.Id,
            user.Email!,
            [.. await userManager.GetRolesAsync(user)],
            [.. await userManager.GetClaimsAsync(user)]
        );
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await GetUserCachedAsync(userId);

        if (user is null)
        {
            logger.LogWarning("Authorization failed for user ID: {UserId} - user not found", userId);
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user);
        var result = await authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await GetUserCachedAsync(userId);

        if (user is null)
        {
            logger.LogWarning("User with ID '{UserId}' not found", userId);
            return ApplicationErrors.UserNotFound;
        }

        return new AppUserDto(
            user.Id,
            user.Email!,
            [.. await userManager.GetRolesAsync(user)],
            [.. await userManager.GetClaimsAsync(user)]
        );
    }

    public async Task<string?> GetUsernameAsync(string userId)
    {
        var user = await GetUserCachedAsync(userId);
        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await GetUserCachedAsync(userId);

        if (user is null)
        {
            logger.LogWarning("User with ID '{UserId}' not found", userId);
            return false;
        }

        return await userManager.IsInRoleAsync(user, role);
    }

    private async Task<AppUser?> GetUserCachedAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guid))
        {
            logger.LogWarning("Invalid user ID format: {UserId}", userId);
            return null;
        }

        return await cache.GetOrCreateAsync(CacheKeys.UserById(guid), async entry =>
        {
            entry.SlidingExpiration = CacheDurations.UserById;
            return await userManager.FindByIdAsync(userId);
        });
    }
}