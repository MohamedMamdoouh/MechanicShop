using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Dashboard.Dtos;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class DashboardControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetTodayStats_WhenAuthenticated_Returns200WithStats()
    {
        var client = await CreateAuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var (response, body) = await client.GetAsync<TodayWorkOrderStatsDto>(
            $"/api/v1/dashboard/today-stats?date={today:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(today, body.Date);
    }

    [Fact]
    public async Task GetTodayStats_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await client.GetAsync(
            $"/api/v1/dashboard/today-stats?date={today:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
