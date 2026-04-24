using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Tests.Common.Employees;
using MediatR;
namespace MechanicShop.Application.SubcutaneousTests.Features.Labors;

public abstract class LaborTestBase(WebAppFactory factory)
{
    protected readonly WebAppFactory Factory = factory;
    protected readonly IMediator Mediator = factory.CreateMediator();

    protected async Task<Employee> SeedLaborAsync()
    {
        var context = Factory.CreateDbContext();
        var labor = EmployeeFactory.CreateLabor().Value;
        context.Employees.Add(labor);
        await context.SaveChangesAsync(CancellationToken.None);
        return labor;
    }
}
