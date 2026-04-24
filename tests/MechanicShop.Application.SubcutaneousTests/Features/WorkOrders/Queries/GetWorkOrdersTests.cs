using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrders;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrdersTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task GetWorkOrders_ReturnsPagedList()
    {
        // Arrange
        await SeedScheduledWorkOrderAsync();
        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 10);

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<PaginatedList<WorkOrderListItemDto>>(result.Value);
        Assert.True(result.Value.Items!.Count >= 1);
    }
}
