using MechanicShop.Application.Features.WorkOrder.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderRepairTasksTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task UpdateWorkOrderRepairTasks_WithValidRepairTasks_Succeeds()
    {
        // Arrange
        var (workOrder, _) = await SeedScheduledWorkOrderBusinessHoursAsync();
        var newRepairTask = await SeedRepairTaskAsync();

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [newRepairTask.Id]);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasks_WithNonExistentWorkOrder_ReturnsNotFoundError()
    {
        // Arrange
        var repairTask = await SeedRepairTaskAsync();
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [repairTask.Id]);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasks_WithNonExistentRepairTask_ReturnsRepairTaskNotFoundError()
    {
        // Arrange
        var (workOrder, _) = await SeedScheduledWorkOrderBusinessHoursAsync();
        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [Guid.NewGuid()]);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.RepairTask.NotFound");
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasks_OnCompletedWorkOrder_ReturnsReadOnlyError()
    {
        // Arrange
        var workOrder = await SeedCompletedWorkOrderAsync();
        var repairTask = await SeedRepairTaskAsync();

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [repairTask.Id]);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.ReadOnly");
    }
}
