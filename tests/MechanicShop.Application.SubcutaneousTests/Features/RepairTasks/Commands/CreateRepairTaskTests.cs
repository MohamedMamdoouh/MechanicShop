using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enum;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateRepairTaskTests(WebAppFactory factory) : RepairTaskTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsRepairTaskDto()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var command = new CreateRepairTaskCommand(
            $"Oil Change-{id}",
            50m,
            RepairDurationMinutes.Min60,
            [new CreateRepairTaskPartCommand($"Oil Filter-{id}", 10m, 1)]);

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.Equal($"Oil Change-{id}", result.Value.Name);
        Assert.Single(result.Value.Parts);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsDuplicateError()
    {
        var existing = await SeedRepairTaskAsync("Duplicate Task");
        var id = Guid.NewGuid().ToString("N")[..8];

        var command = new CreateRepairTaskCommand(
            existing.Name,
            50m,
            RepairDurationMinutes.Min60,
            [new CreateRepairTaskPartCommand($"Part-{id}", 10m, 1)]);

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("RepairTask.Name.Duplicate", result.TopError!.Value.Code);
    }
}
