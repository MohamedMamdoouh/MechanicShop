using MechanicShop.Application.Features.Dashboard.Queries;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatsTests(WebAppFactory factory) : DashboardTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenNoWorkOrdersExistForDate_ReturnsEmptyStats()
    {
        var query = new GetWorkOrderStatsQuery(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));

        var result = await Mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Total);
        Assert.Equal(0m, result.Value.TotalRevenue);
    }

    [Fact]
    public async Task Handle_WhenWorkOrdersExistForToday_ReturnsTotalGreaterThanZero()
    {
        await SeedWorkOrderAsync();

        var query = new GetWorkOrderStatsQuery(DateOnly.FromDateTime(DateTime.UtcNow));

        var result = await Mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Total > 0);
        Assert.True(result.Value.UniqueVehicles > 0);
    }

    [Fact]
    public async Task Handle_WhenDateIsInFuture_ReturnsValidationError()
    {
        var query = new GetWorkOrderStatsQuery(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));

        var result = await Mediator.Send(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Dashboard.Date.Invalid", result.TopError!.Value.Code);
    }
}
