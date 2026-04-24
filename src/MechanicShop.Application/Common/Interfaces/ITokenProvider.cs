using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Dtos;
namespace MechanicShop.Application.Common.Interfaces;

public interface ITokenProvider
{
    public int AccessTokenExpiryMinutes { get; }
    public int RefreshTokenExpiryDays { get; }

    string GenerateAccessToken(AppUserDto user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}