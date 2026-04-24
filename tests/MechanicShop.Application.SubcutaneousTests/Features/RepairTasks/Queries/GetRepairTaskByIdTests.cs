using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdTests(WebAppFactory factory) : RepairTaskTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenRepairTaskExists_ReturnsRepairTaskDto()
    {
        var repairTask = await SeedRepairTaskAsync();

        var result = await Mediator.Send(new GetRepairTaskByIdQuery(repairTask.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(repairTask.Id, result.Value.RepairTaskId);
        Assert.Equal(repairTask.Name, result.Value.Name);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new GetRepairTaskByIdQuery(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.RepairTask.NotFound", result.TopError!.Value.Code);
    }
}
