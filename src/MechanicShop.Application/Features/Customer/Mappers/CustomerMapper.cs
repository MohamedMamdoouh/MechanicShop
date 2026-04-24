using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Domain.Customers.Vehicles;
namespace MechanicShop.Application.Features.Customer.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Domain.Customers.Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.Vehicles.Select(v => v.ToDto()).ToList());
    }

    public static VehicleDto ToDto(this Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        return new(
            vehicle.Id,
            vehicle.Make,
            vehicle.Model,
            vehicle.Year,
            vehicle.LicensePlate);
    }

    public static List<CustomerDto> ToDtoList(this IEnumerable<Domain.Customers.Customer> customers)
    {
        ArgumentNullException.ThrowIfNull(customers);
        return customers.Select(c => c.ToDto()).ToList();
    }

    public static List<VehicleDto> ToDtoList(this IEnumerable<Vehicle> vehicles)
    {
        ArgumentNullException.ThrowIfNull(vehicles);
        return vehicles.Select(v => v.ToDto()).ToList();
    }
}
