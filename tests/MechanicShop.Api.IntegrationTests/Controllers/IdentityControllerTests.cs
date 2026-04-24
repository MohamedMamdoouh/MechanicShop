using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Contracts.Identity;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class IdentityControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        var (email, password) = await Factory.SeedUserAsync();
        var client = CreateUnauthenticatedClient();

        var (Response, Body) = await client.PostAsync<LoginRequest, TokenResponse>(
            "/api/v1/identity/login",
            new LoginRequest(email, password, "device-001"));

        Assert.Equal(HttpStatusCode.OK, Response.StatusCode);
        Assert.NotNull(Body);
        Assert.NotEmpty(Body.AccessToken);
        Assert.NotEmpty(Body.RefreshToken);
        Assert.True(Body.AccessTokenExpiresOnUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var (email, _) = await Factory.SeedUserAsync();
        var client = CreateUnauthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/identity/login",
            new LoginRequest(email, "WrongPassword!", "device-001"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/identity/login",
            new LoginRequest("nobody@example.com", "SomePassword!", "device-001"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WhenAuthenticated_Returns200WithUserInfo()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/identity/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/identity/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_Returns204()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsync(
            "/api/v1/identity/logout",
            new LogoutRequest("device-001"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/identity/logout",
            new LogoutRequest("device-001"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
