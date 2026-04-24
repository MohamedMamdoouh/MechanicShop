using MechanicShop.Application.Features.WorkOrder.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enum;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderStateTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task UpdateWorkOrderState_ScheduledToInProgress_Succeeds()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync();
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateWorkOrderState_InProgressToCompleted_Succeeds()
    {
        // Arrange
        var workOrder = await SeedInProgressWorkOrderAsync();
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Completed);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateWorkOrderState_ScheduledToCancelled_Succeeds()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync();
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Cancelled);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateWorkOrderState_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }

    [Fact]
    public async Task UpdateWorkOrderState_InvalidTransition_ReturnsError()
    {
        // Arrange — a Completed work order cannot transition to any state
        var workOrder = await SeedCompletedWorkOrderAsync();
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
