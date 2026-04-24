using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
namespace MechanicShop.Application.Features.Identity.Commands.RefreshTokens;

public sealed class RefreshTokenCommandHandler(
    ILogger<RefreshTokenCommandHandler> logger,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    ITokenSessionService tokenSessionService)
    : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate the expired access token and extract the user ID claim (without validating expiry)
        var principal = tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if (principal is null)
        {
            logger.LogWarning("Invalid expired access token provided for refresh.");
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null)
        {
            logger.LogWarning("User ID claim not found in expired access token.");
            return ApplicationErrors.UserIdClaimInvalid;
        }

        // 2. Validate the refresh token via session service (handles hash lookup, expiry, fingerprint, reuse detection)
        var refreshToken = await tokenSessionService.GetValidTokenAsync(
            request.RefreshToken,
            userIdClaim,
            request.DeviceIdentifier,
            request.UserAgent,
            cancellationToken);

        if (refreshToken is null)
        {
            logger.LogWarning("Refresh token validation failed for user ID {UserId}.", userIdClaim);
            return ApplicationErrors.InvalidRefreshToken;
        }

        // 3. Get the user to generate a new access token
        var userResult = await identityService.GetUserByIdAsync(userIdClaim, cancellationToken);

        if (!userResult.IsSuccess)
        {
            logger.LogWarning(
                "User with ID {UserId} not found during token refresh.",
                userIdClaim);

            return ApplicationErrors.UserNotFound;
        }

        var newAccessToken = tokenProvider.GenerateAccessToken(userResult.Value);

        // 4. Rotate the refresh token session
        var device = new DeviceInfo(request.DeviceIdentifier, request.UserAgent, request.IpAddress);
        var (newRefreshToken, rawRefreshToken) = await tokenSessionService.RotateAsync(
            refreshToken,
            device,
            cancellationToken);

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(tokenProvider.AccessTokenExpiryMinutes),
            RefreshTokenExpiresOnUtc = newRefreshToken.ExpiresOnUtc
        };
    }
}