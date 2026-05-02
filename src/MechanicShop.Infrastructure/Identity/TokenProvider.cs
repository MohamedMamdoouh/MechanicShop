using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Infrastructure.Identity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace MechanicShop.Infrastructure.Identity;

public sealed class TokenProvider(IOptions<JwtSettings> jwtSettings, ILogger<TokenProvider> logger) : ITokenProvider
{
    public int AccessTokenExpiryMinutes => jwtSettings.Value.AccessTokenExpiryMinutes;
    public int RefreshTokenExpiryDays => jwtSettings.Value.RefreshTokenExpiryDays;

    public string GenerateAccessToken(AppUserDto user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Value.SecretKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTimeOffset.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpiryMinutes).UtcDateTime,
            Issuer = jwtSettings.Value.Issuer,
            Audience = jwtSettings.Value.Audience,
            SigningCredentials = creds
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = jwtSettings.Value.Audience,

            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Value.Issuer,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Value.SecretKey)),

            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();

        try
        {
            return handler.ValidateToken(token, parameters, out _);
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Failed to validate expired token");
            return null;
        }
    }
}
