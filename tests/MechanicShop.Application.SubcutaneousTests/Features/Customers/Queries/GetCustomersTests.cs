using MechanicShop.Application.Features.Customer.Queries.GetCustomers;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomersTests(WebAppFactory factory) : CustomerTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenCustomersExist_ReturnsPaginatedList()
    {
        await SeedCustomerAsync();
        await SeedCustomerAsync();

        var result = await Mediator.Send(new GetCustomersQuery(PageNumber: 1, PageSize: 10));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalCount >= 2);
        Assert.NotEmpty(result.Value.Items!);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPageSize()
    {
        await SeedCustomerAsync();
        await SeedCustomerAsync();
        await SeedCustomerAsync();

        var result = await Mediator.Send(new GetCustomersQuery(PageNumber: 1, PageSize: 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items!.Count);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(2, result.Value.PageSize);
    }

    [Fact]
    public async Task Handle_OnFirstPage_HasNextPageTrueAndHasPreviousPageFalse()
    {
        // Seed enough to guarantee at least 2 pages at page size 2
        await SeedCustomerAsync();
        await SeedCustomerAsync();
        await SeedCustomerAsync();

        var result = await Mediator.Send(new GetCustomersQuery(PageNumber: 1, PageSize: 2));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalPages > 1);
        Assert.False(result.Value.HasPreviousPage);
        Assert.True(result.Value.HasNextPage);
    }

    [Fact]
    public async Task Handle_OnLastPage_HasNextPageFalseAndHasPreviousPageTrue()
    {
        // Seed enough to guarantee at least 2 pages at page size 2
        await SeedCustomerAsync();
        await SeedCustomerAsync();
        await SeedCustomerAsync();

        var probe = await Mediator.Send(new GetCustomersQuery(PageNumber: 1, PageSize: 2));
        Assert.True(probe.IsSuccess);
        var lastPage = probe.Value.TotalPages;
        Assert.True(lastPage > 1);

        var result = await Mediator.Send(new GetCustomersQuery(PageNumber: lastPage, PageSize: 2));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasPreviousPage);
        Assert.False(result.Value.HasNextPage);
    }
}
