using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler(
    ILogger<LogoutCommandHandler> logger,
    ITokenSessionService tokenSessionService)
    : IRequestHandler<LogoutCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await tokenSessionService.RevokeDeviceSessionAsync(
            request.UserId,
            request.DeviceIdentifier,
            cancellationToken);

        logger.LogInformation(
            "Device session revoked for user {UserId} on device {DeviceId}",
            request.UserId,
            request.DeviceIdentifier);

        return Result.Deleted;
    }
}
