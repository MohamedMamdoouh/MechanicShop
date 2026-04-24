using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Identity.Commands.RefreshTokens;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string ExpiredAccessToken,
    string DeviceIdentifier,
    string? UserAgent = null,
    string? IpAddress = null)
    : IRequest<Result<TokenResponse>>;