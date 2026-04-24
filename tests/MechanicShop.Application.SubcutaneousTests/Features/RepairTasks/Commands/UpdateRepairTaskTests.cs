using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enum;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskTests(WebAppFactory factory) : RepairTaskTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenRepairTaskExists_ReturnsSuccess()
    {
        var repairTask = await SeedRepairTaskAsync();
        var id = Guid.NewGuid().ToString("N")[..8];

        var command = new UpdateRepairTaskCommand(
            repairTask.Id,
            $"Updated Task {id}",
            75m,
            RepairDurationMinutes.Min90,
            [new UpdateRepairTaskPartCommand(Guid.Empty, $"Part-{id}", 25m, 1)]);

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ReturnsNotFoundError()
    {
        var command = new UpdateRepairTaskCommand(
            Guid.NewGuid(),
            "Non-existent Task",
            50m,
            RepairDurationMinutes.Min60,
            []);

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.RepairTask.NotFound", result.TopError!.Value.Code);
    }
}
