using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.Customers.Vehicles;

public class Vehicle : AuditableEntity
{
    public Guid CustomerId { get; init; }
    public Customer Customer { get; } = null!;
    public string Make { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public int Year { get; private set; }
    public string LicensePlate { get; private set; } = default!;

    public string VehicleInfo => $"{Year} | {Make} | {Model} ({LicensePlate})";

    // Parameterless constructor for EF Core and other ORMs
    private Vehicle() { }

    private Vehicle(
        string make,
        string model,
        int year,
        string licensePlate)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    public static Result<Vehicle> Create(
        string make,
        string model,
        int year,
        string licensePlate)
    {
        var errors = Validate(make, model, year, licensePlate);

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Vehicle(
            make.Trim(),
            model.Trim(),
            year,
            licensePlate.Trim());
    }

    public Result<Updated> Update(
        string make,
        string model,
        int year,
        string licensePlate)
    {
        var errors = Validate(make, model, year, licensePlate);

        if (errors.Count > 0)
        {
            return errors;
        }

        Make = make.Trim();
        Model = model.Trim();
        Year = year;
        LicensePlate = licensePlate.Trim();

        return Result.Updated;
    }

    private static List<Error> Validate(string make, string model, int year, string licensePlate)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(make))
        {
            errors.Add(VehicleErrors.MakeRequired);
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            errors.Add(VehicleErrors.ModelRequired);
        }

        if (year < 1886 || year > DateTimeOffset.UtcNow.Year + 1)
        {
            errors.Add(VehicleErrors.YearInvalid);
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            errors.Add(VehicleErrors.LicensePlateRequired);
        }

        return errors;
    }
}