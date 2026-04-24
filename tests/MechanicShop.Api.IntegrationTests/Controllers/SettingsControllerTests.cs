using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Settings;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class SettingsControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetShopSettings_WhenAnonymous_Returns200WithSettings()
    {
        var client = CreateUnauthenticatedClient();

        var (response, body) = await client.GetAsync<ShopSettingsDto>("/api/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetShopSettings_WhenAuthenticated_Returns200WithSettings()
    {
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<ShopSettingsDto>("/api/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEmpty(body!.ShopName);
    }
}
