using MechanicShop.Application.Features.WorkOrder.Commands.DeleteWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DeleteWorkOrderTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task DeleteWorkOrder_WithScheduledWorkOrder_Succeeds()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync();
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteWorkOrder_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }

    [Fact]
    public async Task DeleteWorkOrder_WithInProgressWorkOrder_ReturnsReadOnlyError()
    {
        // Arrange
        var workOrder = await SeedInProgressWorkOrderAsync();
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.ReadOnly");
    }

    [Fact]
    public async Task DeleteWorkOrder_WithCompletedWorkOrder_ReturnsReadOnlyError()
    {
        // Arrange
        var workOrder = await SeedCompletedWorkOrderAsync();
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.ReadOnly");
    }
}
