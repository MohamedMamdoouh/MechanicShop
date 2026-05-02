using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Dtos;
namespace MechanicShop.Application.Common.Interfaces;

public interface ITokenProvider
{
    int AccessTokenExpiryMinutes { get; }
    int RefreshTokenExpiryDays { get; }

    string GenerateAccessToken(AppUserDto user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}