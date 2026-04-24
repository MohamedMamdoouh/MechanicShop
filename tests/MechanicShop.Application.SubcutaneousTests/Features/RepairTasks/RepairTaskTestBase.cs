using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks;

public abstract class RepairTaskTestBase(WebAppFactory factory)
{
    protected readonly WebAppFactory Factory = factory;
    protected readonly IMediator Mediator = factory.CreateMediator();

    protected async Task<RepairTask> SeedRepairTaskAsync(string? name = null)
    {
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];
        var repairTask = RepairTaskFactory.Create(name: name ?? $"Task-{id}").Value;
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);
        return repairTask;
    }
}
