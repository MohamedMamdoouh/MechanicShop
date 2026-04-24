using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Identity.Queries;

public sealed class GetUserInfoQueryHandler(
    ILogger<GetUserInfoQueryHandler> logger,
    IIdentityService identityService)
    : IRequestHandler<GetUserInfoQuery, Result<AppUserDto>>
{
    public async Task<Result<AppUserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var userResult = await identityService.GetUserByIdAsync(request.UserId, cancellationToken);

        if (!userResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to retrieve user info for UserId: {UserId}. Reason: {Reason}",
                request.UserId,
                userResult.TopError);

            return userResult.Errors.ToList();
        }

        return userResult.Value;
    }
}