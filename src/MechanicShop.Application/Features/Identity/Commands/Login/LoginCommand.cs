using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Identity.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string DeviceIdentifier,
    string? UserAgent = null,
    string? IpAddress = null)
    : IRequest<Result<TokenResponse>>;