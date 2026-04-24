using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.Employees;

public static class EmployeeErrors
{
    public static Error FirstNameRequired
    => Error.Validation("Employee first name is required.", "Employee.FirstName.Required");

    public static Error LastNameRequired
    => Error.Validation("Employee last name is required.", "Employee.LastName.Required");

    public static Error RoleInvalid
    => Error.Validation("Employee role is invalid.", "Employee.Role.Invalid");

    public static Error NotFound
    => Error.NotFound("Employee not found.", "Employee.NotFound");
}