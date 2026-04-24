using MechanicShop.Tests.Common.Customers;
using Xunit;
namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = CustomerFactory.CreateCustomer();
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Vehicles);
    }

    [Fact]
    public void Create_ShouldFail_WithInvalidEmail()
    {
        var result = CustomerFactory.CreateCustomer(email: "invalid-email");
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "Customer.Email.Invalid");
    }

    [Fact]
    public void Update_ShouldSucceed_WithValidData()
    {
        var result = CustomerFactory.CreateCustomer();
        var updateResult = result.Value.Update("Jane", "Smith", "jane@example.com", "+201098765432");
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Jane", result.Value.FirstName);
        Assert.Equal("Smith", result.Value.LastName);
        Assert.Equal("jane@example.com", result.Value.Email);
        Assert.Equal("+201098765432", result.Value.PhoneNumber);
    }

    [Fact]
    public void AddVehicle_ShouldAddSuccessfully()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = VehicleFactory.CreateVehicle().Value;
        customer.AddVehicle(vehicle);
        Assert.Equal(2, customer.Vehicles.Count);
    }

    [Fact]
    public void Create_ShouldFail_WithPhoneEmpty()
    {
        var result = CustomerFactory.CreateCustomer(phoneNumber: "");
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "Customer.PhoneNumber.Required");
    }
}