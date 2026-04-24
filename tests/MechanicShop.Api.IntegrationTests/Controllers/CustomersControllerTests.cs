using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Contracts.Customers;
using MechanicShop.Domain.Identity;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class CustomersControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetCustomers_WhenAuthenticated_Returns200WithList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<PaginatedList<CustomerDto>>("/api/v1/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetCustomers_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WhenExists_Returns200WithCustomer()
    {
        var seeded = await SeedCustomerAsync();
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<CustomerDto>($"/api/v1/customers/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(seeded.Id, body.Id);
    }

    [Fact]
    public async Task GetCustomerById_WhenNotFound_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_AsManager_Returns201WithCustomer()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var (response, body) = await client.PostAsync<CreateCustomerRequest, CustomerDto>(
            "/api/v1/customers",
            new CreateCustomerRequest(
                "Jane",
                "Smith",
                $"{id}@test.com",
                CreateUniquePhoneNumber(),
                [new CreateVehicleRequest("Toyota", "Corolla", 2022, $"CR-{id[..6]}")]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Jane", body.FirstName);
        Assert.Equal("Smith", body.LastName);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateCustomer_AsLabor_Returns403()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = await CreateAuthenticatedClientAsync(Role.Labor);

        var response = await client.PostAsync(
            "/api/v1/customers",
            new CreateCustomerRequest(
                "Jane",
                "Smith",
                $"{id}@test.com",
                CreateUniquePhoneNumber(),
                [new CreateVehicleRequest("Toyota", "Corolla", 2022, $"LB-{id[..6]}")]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WhenUnauthenticated_Returns401()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = CreateUnauthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/customers",
            new CreateCustomerRequest(
                "Jane",
                "Smith",
                $"{id}@test.com",
                CreateUniquePhoneNumber(),
                [new CreateVehicleRequest("Toyota", "Corolla", 2022, $"UN-{id[..6]}")]));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_Returns409()
    {
        var seeded = await SeedCustomerAsync();
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.PostAsync(
            "/api/v1/customers",
            new CreateCustomerRequest(
                "Other",
                "User",
                seeded.Email,
                CreateUniquePhoneNumber(),
                [new CreateVehicleRequest("Honda", "Civic", 2021, $"DP-{id[..6]}")]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_AsManager_Returns204()
    {
        var seeded = await SeedCustomerAsync();
        var client = await CreateAuthenticatedClientAsync(Role.Manager);
        var id = Guid.NewGuid().ToString("N")[..8];

        var response = await client.PutAsync(
            "/api/v1/customers",
            new UpdateCustomerRequest(
                seeded.Id,
                "Updated",
                "Name",
                $"upd-{id}@test.com",
                CreateUniquePhoneNumber()));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_AsManager_Returns204()
    {
        var seeded = await SeedCustomerAsync();
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.DeleteAsync($"/api/v1/customers/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WhenNotFound_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.DeleteAsync($"/api/v1/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static string CreateUniquePhoneNumber()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        var sequence = BitConverter.ToUInt32(bytes, 0) % 100_000_000;
        return $"+2015{sequence:D8}";
    }
}
