using MechanicShop.Application.Features.WorkOrder.Commands.AssignLabor;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AssignLaborTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task AssignLabor_WithValidLaborNotOccupied_Succeeds()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync();
        var newLabor = await SeedLaborAsync();

        // Act
        var command = new AssignLaborCommand(workOrder.Id, newLabor.Id);
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AssignLabor_WithNonExistentWorkOrder_ReturnsNotFoundError()
    {
        // Arrange
        var newLabor = await SeedLaborAsync();
        var command = new AssignLaborCommand(Guid.NewGuid(), newLabor.Id);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }

    [Fact]
    public async Task AssignLabor_WithNonExistentLabor_ReturnsNotFoundError()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync();
        var command = new AssignLaborCommand(workOrder.Id, Guid.NewGuid());

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.Labor.NotFound");
    }

    [Fact]
    public async Task AssignLabor_WithOccupiedLabor_ReturnsLaborOccupiedError()
    {
        var workOrder1 = await SeedScheduledWorkOrderAsync();
        var workOrder2 = await SeedScheduledWorkOrderAsync();

        var command = new AssignLaborCommand(workOrder2.Id, workOrder1.LaborId);

        // Act
        var result = await Mediator.Send(command);

        // Assert — labor is occupied because workOrder1 uses it during the same window
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.Labor.Occupied");
    }
}
