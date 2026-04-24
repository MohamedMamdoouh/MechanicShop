using Asp.Versioning;
using MechanicShop.Api;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Commands.Login;
using MechanicShop.Contracts.Identity;
using MechanicShop.Application.Features.Identity.Commands.Logout;
using MechanicShop.Application.Features.Identity.Commands.RefreshTokens;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public sealed class IdentityController(ISender sender, IUser currentUser) : ApiController
{
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Login")]
    [EndpointDescription("Authenticates a user and returns an access token and a refresh token.")]
    [EndpointName("Login")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.DeviceIdentifier,
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await sender.Send(command, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitPolicies.Refresh)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Refresh tokens")]
    [EndpointDescription("Issues a new access token and rotates the refresh token for the given device session.")]
    [EndpointName("RefreshTokens")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokensRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(
            request.RefreshToken,
            request.ExpiredAccessToken,
            request.DeviceIdentifier,
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await sender.Send(command, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get current user info")]
    [EndpointDescription("Returns the profile and roles of the currently authenticated user.")]
    [EndpointName("GetCurrentUser")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var query = new GetUserInfoQuery(currentUser.Id);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Logout")]
    [EndpointDescription("Revokes the refresh token session for the given device, signing the user out on that device only.")]
    [EndpointName("Logout")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var command = new LogoutCommand(currentUser.Id, request.DeviceIdentifier);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }
}


