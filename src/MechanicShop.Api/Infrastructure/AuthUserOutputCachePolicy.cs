using System.Security.Claims;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.JsonWebTokens;
namespace MechanicShop.Api.Infrastructure;

public sealed class AuthUserOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var method = context.HttpContext.Request.Method;
        if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method))
        {
            context.EnableOutputCaching = false;
            return ValueTask.CompletedTask;
        }

        var userId = context.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId is not null)
        {
            context.CacheVaryByRules.VaryByValues.TryAdd("uid", userId);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;
}
