using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Domain.Employees;
using MechanicShop.Tests.Common.Employees;
using Xunit;
namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllFieldsCorrectly()
    {
        var employee = EmployeeFactory.CreateLabor(firstName: "Jane", lastName: "Smith").Value;

        var dto = employee.ToDto();

        Assert.Equal(employee.Id, dto.Id);
        Assert.Equal(employee.FullName, dto.Name);
    }

    [Fact]
    public void ToDtos_ShouldMapAllEmployeesCorrectly()
    {
        var e1 = EmployeeFactory.CreateLabor(firstName: "Alice", lastName: "A").Value;
        var e2 = EmployeeFactory.CreateLabor(firstName: "Bob", lastName: "B").Value;

        var dtos = new List<Employee> { e1, e2 }.ToDtos();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(e1.Id, dtos[0].Id);
        Assert.Equal(e1.FullName, dtos[0].Name);
        Assert.Equal(e2.Id, dtos[1].Id);
        Assert.Equal(e2.FullName, dtos[1].Name);
    }

    [Fact]
    public void ToDtos_ShouldReturnEmpty_WhenSourceIsEmpty()
    {
        var dtos = new List<Employee>().ToDtos();
        Assert.Empty(dtos);
    }
}