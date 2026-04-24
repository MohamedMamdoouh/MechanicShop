using System.Security.Claims;
namespace MechanicShop.Application.Features.Identity.Dtos;

public sealed record AppUserDto(
    string UserId,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Claim> Claims
);