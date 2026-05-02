using System.Net.Mail;
using System.Text.RegularExpressions;
using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
namespace MechanicShop.Domain.Customers;

public class Customer : AuditableEntity
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;

    private readonly List<Vehicle> _vehicles = [];
    public IReadOnlyCollection<Vehicle> Vehicles => _vehicles.AsReadOnly();

    // Parameterless constructor for EF Core and other ORMs
    private Customer() { }

    private Customer(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        List<Vehicle> vehicles)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        _vehicles = vehicles;
    }

    public static Result<Customer> Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        List<Vehicle> vehicles)
    {
        var errors = Validate(firstName, lastName, email, phoneNumber);

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Customer(
            firstName.Trim(),
            lastName.Trim(),
            email.Trim(),
            phoneNumber.Trim(),
            vehicles);
    }

    public void AddVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        _vehicles.Add(vehicle);
    }

    public Result<Updated> Update(
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        var errors = Validate(firstName, lastName, email, phoneNumber);

        if (errors.Count > 0)
        {
            return errors;
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        PhoneNumber = phoneNumber.Trim();

        return Result.Updated;
    }

    private static List<Error> Validate(
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add(CustomerErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add(CustomerErrors.LastNameRequired);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add(CustomerErrors.EmailRequired);
        }
        else if (!MailAddress.TryCreate(email, out _))
        {
            errors.Add(CustomerErrors.EmailInvalid);
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            errors.Add(CustomerErrors.PhoneNumberRequired);
        }

        return errors;
    }
}
