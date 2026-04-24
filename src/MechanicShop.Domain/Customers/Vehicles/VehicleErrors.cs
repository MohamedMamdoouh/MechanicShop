using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.Customers.Vehicles;

public static class VehicleErrors
{
    public static Error MakeRequired
        => Error.Validation("Vehicle make is required.", "Vehicle.Make.Required");

    public static Error ModelRequired
        => Error.Validation("Vehicle model is required.", "Vehicle.Model.Required");

    public static Error YearInvalid
        => Error.Validation("Vehicle year is invalid.", "Vehicle.Year.Invalid");

    public static Error NotFound
        => Error.NotFound("Vehicle not found.", "Vehicle.NotFound");

    public static Error LicensePlateRequired
           => Error.Validation("Vehicle license plate is required.", "Vehicle.LicensePlate.Required");
}