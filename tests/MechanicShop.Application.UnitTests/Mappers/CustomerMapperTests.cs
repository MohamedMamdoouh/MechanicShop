using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTests
{
    // --- Customer.ToDto ---

    [Fact]
    public void CustomerToDto_ShouldMapAllFieldsCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;

        var dto = customer.ToDto();

        Assert.Equal(customer.Id, dto.Id);
        Assert.Equal(customer.FirstName, dto.FirstName);
        Assert.Equal(customer.LastName, dto.LastName);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
        Assert.Single(dto.Vehicles);
        Assert.Equal(vehicle.Id, dto.Vehicles[0].Id);
    }

    [Fact]
    public void CustomerToDto_ShouldThrow_WhenCustomerIsNull()
    {
        Customer? customer = null;
        Assert.Throws<ArgumentNullException>(() => customer!.ToDto());
    }

    // --- Vehicle.ToDto ---

    [Fact]
    public void VehicleToDto_ShouldMapAllFieldsCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle(
            make: "Toyota",
            model: "Camry",
            year: 2022,
            licensePlate: "XYZ-9999").Value;

        var dto = vehicle.ToDto();

        Assert.Equal(vehicle.Id, dto.Id);
        Assert.Equal("Toyota", dto.Make);
        Assert.Equal("Camry", dto.Model);
        Assert.Equal(2022, dto.Year);
        Assert.Equal("XYZ-9999", dto.LicensePlate);
    }

    [Fact]
    public void VehicleToDto_ShouldThrow_WhenVehicleIsNull()
    {
        Vehicle? vehicle = null;
        Assert.Throws<ArgumentNullException>(() => vehicle!.ToDto());
    }

    // --- Customer.ToDtoList ---

    [Fact]
    public void CustomerToDtoList_ShouldMapAllCustomersCorrectly()
    {
        var c1 = CustomerFactory.CreateCustomer(firstName: "Alice").Value;
        var c2 = CustomerFactory.CreateCustomer(firstName: "Bob", email: "bob@example.com").Value;

        var dtos = new List<Customer> { c1, c2 }.ToDtoList();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(c1.Id, dtos[0].Id);
        Assert.Equal(c2.Id, dtos[1].Id);
    }

    [Fact]
    public void CustomerToDtoList_ShouldReturnEmpty_WhenSourceIsEmpty()
    {
        var dtos = new List<Customer>().ToDtoList();
        Assert.Empty(dtos);
    }

    [Fact]
    public void CustomerToDtoList_ShouldThrow_WhenCustomersIsNull()
    {
        IEnumerable<Customer>? customers = null;
        Assert.Throws<ArgumentNullException>(() => customers!.ToDtoList());
    }

    // --- Vehicle.ToDtoList ---

    [Fact]
    public void VehicleToDtoList_ShouldMapAllVehiclesCorrectly()
    {
        var v1 = VehicleFactory.CreateVehicle(make: "Ford").Value;
        var v2 = VehicleFactory.CreateVehicle(make: "BMW").Value;

        var dtos = new List<Vehicle> { v1, v2 }.ToDtoList();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(v1.Id, dtos[0].Id);
        Assert.Equal(v2.Id, dtos[1].Id);
    }

    [Fact]
    public void VehicleToDtoList_ShouldThrow_WhenVehiclesIsNull()
    {
        IEnumerable<Vehicle>? vehicles = null;
        Assert.Throws<ArgumentNullException>(() => vehicles!.ToDtoList());
    }
}