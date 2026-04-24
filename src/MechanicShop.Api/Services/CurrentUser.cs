using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
namespace MechanicShop.Api.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string Id =>
        httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? string.Empty;
}