using MechanicShop.Tests.Common.Customers;
using Xunit;
namespace MechanicShop.Domain.UnitTests.Customers;

public class VehicleTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = VehicleFactory.CreateVehicle();
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ShouldFail_WithMissingMake()
    {
        var result = VehicleFactory.CreateVehicle(make: "");
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(1700)]
    [InlineData(3000)]
    public void Create_ShouldFail_WithInvalidYear(int year)
    {
        var result = VehicleFactory.CreateVehicle(year: year);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldFail_WithInvalidLicensePlate(string licensePlate)
    {
        var result = VehicleFactory.CreateVehicle(licensePlate: licensePlate);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void VehicleInfo_ShouldReturnFormattedString()
    {
        var result = VehicleFactory.CreateVehicle(make: "Toyota", model: "Camry", year: 2020, licensePlate: "ABC123");
        Assert.True(result.IsSuccess);
        Assert.Equal("2020 | Toyota | Camry (ABC123)", result.Value.VehicleInfo);
    }
}