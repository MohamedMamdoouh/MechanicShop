using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Employees;
using Xunit;
namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = EmployeeFactory.CreateEmployee();
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Update_ShouldSucceed()
    {
        var employee = EmployeeFactory.CreateEmployee().Value;
        var updateResult = employee.Update("Jane", "Doe", Role.Labor);
        Assert.True(updateResult.IsSuccess);
    }

    [Fact]
    public void Update_ShouldFail_WithEmptyFirstName()
    {
        var employee = EmployeeFactory.CreateEmployee().Value;
        var updateResult = employee.Update("", "Doe", Role.Labor);
        Assert.False(updateResult.IsSuccess);
    }
}