using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Labor.Dtos;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class LaborsControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetLabors_WhenAuthenticated_Returns200WithList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<List<LaborDto>>("/api/v1/labors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetLabors_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/labors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
