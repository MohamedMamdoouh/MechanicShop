using MechanicShop.Application.Features.Labor.Queries;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Employees;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetLaborsTests(WebAppFactory factory) : LaborTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenLaborsExist_ReturnsLaborList()
    {
        var labor = await SeedLaborAsync();

        var result = await Mediator.Send(new GetLaborsQuery());

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.All(result.Value, labor => Assert.False(string.IsNullOrWhiteSpace(labor.Name)));
    }

    [Fact]
    public async Task Handle_WhenManagerSeeded_DoesNotReturnManager()
    {
        var context = Factory.CreateDbContext();
        var manager = EmployeeFactory.CreateManager().Value;
        context.Employees.Add(manager);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Mediator.Send(new GetLaborsQuery());

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, labor => labor.Id == manager.Id);
    }
}
