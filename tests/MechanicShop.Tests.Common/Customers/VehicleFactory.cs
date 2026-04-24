using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
namespace MechanicShop.Tests.Common.Customers;

public static class VehicleFactory
{
    public static Result<Vehicle> CreateVehicle(
        string? make = null,
        string? model = null,
        int? year = null,
        string? licensePlate = null)
    {
        var id = Guid.NewGuid().ToString("N")[..6];
        return Vehicle.Create(
            make ?? "Toyota",
            model ?? "Camry",
            year ?? 2020,
            licensePlate ?? $"ABC{id}");
    }
}