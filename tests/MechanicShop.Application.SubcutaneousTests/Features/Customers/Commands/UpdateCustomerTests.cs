using MechanicShop.Application.Features.Customer.Commands.UpdateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateCustomerTests(WebAppFactory factory) : CustomerTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsSuccess()
    {
        var customer = await SeedCustomerAsync();
        var id = Guid.NewGuid().ToString("N")[..8];

        var command = new UpdateCustomerCommand(
            customer.Id,
            "Updated",
            "Name",
            $"updated-{id}@test.com",
            "+201098765432");

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ReturnsNotFoundError()
    {
        var command = new UpdateCustomerCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            "nobody@test.com",
            "+201012345678");

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Customer.NotFound", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenEmailTakenByAnotherCustomer_ReturnsConflictError()
    {
        var first = await SeedCustomerAsync();
        var second = await SeedCustomerAsync();

        var command = new UpdateCustomerCommand(
            second.Id,
            second.FirstName,
            second.LastName,
            first.Email,
            second.PhoneNumber);

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Customer.Email.AlreadyExists", result.TopError!.Value.Code);
    }
}
