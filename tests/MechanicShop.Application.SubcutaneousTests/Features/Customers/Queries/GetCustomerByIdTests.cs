using MechanicShop.Application.Features.Customer.Queries.GetCustomerById;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomerByIdTests(WebAppFactory factory) : CustomerTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsCustomerDto()
    {
        var customer = await SeedCustomerAsync();

        var result = await Mediator.Send(new GetCustomerByIdQuery(customer.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(customer.Id, result.Value.Id);
        Assert.Equal(customer.FirstName, result.Value.FirstName);
        Assert.Equal(customer.Email, result.Value.Email);
        Assert.NotEmpty(result.Value.Vehicles);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Customer.NotFound", result.TopError!.Value.Code);
    }
}
