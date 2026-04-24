using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTasksTests(WebAppFactory factory) : RepairTaskTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenRepairTasksExist_ReturnsAllRepairTasks()
    {
        await SeedRepairTaskAsync();
        await SeedRepairTaskAsync();

        var result = await Mediator.Send(new GetRepairTasksQuery());

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.All(result.Value, rt => Assert.False(string.IsNullOrWhiteSpace(rt.Name)));
    }
}
