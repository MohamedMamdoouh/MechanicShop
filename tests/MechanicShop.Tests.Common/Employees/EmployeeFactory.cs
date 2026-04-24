using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
namespace MechanicShop.Tests.Common.Employees;

public static class EmployeeFactory
{
    public static Result<Employee> CreateEmployee(
        string? firstName = null,
        string? lastName = null,
        Role? role = null)
    {
        return Employee.Create(
            firstName ?? "John",
            lastName ?? "Employee",
            role ?? Role.Labor);
    }

    public static Result<Employee> CreateLabor(
       string? firstName = null,
       string? lastName = null)
    {
        return Employee.Create(
            firstName ?? "John",
            lastName ?? "Labor",
            Role.Labor);
    }

    public static Result<Employee> CreateManager(
    string? firstName = null,
    string? lastName = null)
    {
        return Employee.Create(
            firstName ?? "John",
            lastName ?? "Manager",
            Role.Manager);
    }
}