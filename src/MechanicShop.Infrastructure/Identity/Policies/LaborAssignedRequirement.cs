using System.Security.Claims;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
namespace MechanicShop.Infrastructure.Identity.Policies;

public class LaborAssignedRequirement : IAuthorizationRequirement { }

public class LaborAssignedHandler : AuthorizationHandler<LaborAssignedRequirement, WorkOrder>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LaborAssignedRequirement requirement,
        WorkOrder resource)
    {
        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var userGuid))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (resource.LaborId == userGuid || context.User.IsInRole(nameof(Role.Manager)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this, "User is not the assigned labor or a manager."));
        }

        return Task.CompletedTask;
    }
}