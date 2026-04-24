using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Identity.Commands.Login;

public sealed class LoginCommandHandler(
    ILogger<LoginCommandHandler> logger,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IRefreshTokenFactory refreshTokenFactory,
    ITokenSessionService tokenSessionService)
    : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userResponse = await identityService.AuthenticateAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!userResponse.IsSuccess)
        {
            logger.LogWarning(
                "Authentication failed for email {Email}. Reason: {Reason}",
                Common.Utilities.UtilityService.MaskEmail(request.Email),
                userResponse.TopError);

            return ApplicationErrors.AuthenticationFailed;
        }

        var user = userResponse.Value;
        var userId = Guid.Parse(user.UserId);

        var accessToken = tokenProvider.GenerateAccessToken(user);

        var deviceInfo = new DeviceInfo(
            request.DeviceIdentifier,
            request.UserAgent,
            request.IpAddress);

        var (refreshToken, rawToken) = refreshTokenFactory.Create(
            userId,
            deviceInfo,
            DateTimeOffset.UtcNow.AddDays(tokenProvider.RefreshTokenExpiryDays));

        await tokenSessionService.ReplaceDeviceSessionAsync(
            userId.ToString(),
            deviceInfo,
            refreshToken,
            cancellationToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            AccessTokenExpiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(tokenProvider.AccessTokenExpiryMinutes),
            RefreshTokenExpiresOnUtc = refreshToken.ExpiresOnUtc
        };
    }
}