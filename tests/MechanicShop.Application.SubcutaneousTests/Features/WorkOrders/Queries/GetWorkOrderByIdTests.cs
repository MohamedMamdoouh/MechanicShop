using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrderById;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task GetWorkOrderById_WithExistingId_ReturnsWorkOrderDto()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync();
        var query = new GetWorkOrderByIdQuery(workOrder.Id);

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<WorkOrderDto>(result.Value);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
    }

    [Fact]
    public async Task GetWorkOrderById_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetWorkOrderByIdQuery(Guid.NewGuid());

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }
}
