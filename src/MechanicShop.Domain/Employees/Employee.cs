using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
namespace MechanicShop.Domain.Employees;

public class Employee : AuditableEntity
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public Role Role { get; private set; }

    // Parameterless constructor for EF Core and other ORMs
    private Employee() { }

    private Employee(string firstName, string lastName, Role role)
    {
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }

    public static Result<Employee> Create(string firstName, string lastName, Role role)
    {
        var errors = Validate(firstName, lastName, role);

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Employee(firstName.Trim(), lastName.Trim(), role);
    }

    public Result<Updated> Update(string firstName, string lastName, Role role)
    {
        var errors = Validate(firstName, lastName, role);

        if (errors.Count > 0)
        {
            return errors;
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Role = role;

        return Result.Updated;
    }

    private static List<Error> Validate(string firstName, string lastName, Role role)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add(EmployeeErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add(EmployeeErrors.LastNameRequired);
        }

        if (!Enum.IsDefined(role))
        {
            errors.Add(EmployeeErrors.RoleInvalid);
        }

        return errors;
    }
}