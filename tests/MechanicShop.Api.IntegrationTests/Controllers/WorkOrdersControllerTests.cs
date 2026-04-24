using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Contracts.WorkOrders;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class WorkOrdersControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetWorkOrders_WhenAuthenticated_Returns200WithPaginatedList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<PaginatedList<WorkOrderListItemDto>>("/api/v1/workorders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetWorkOrders_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/workorders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkOrderById_WhenExists_Returns200WithWorkOrder()
    {
        var seeded = await SeedWorkOrderAsync();
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<WorkOrderDto>($"/api/v1/workorders/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(seeded.Id, body!.WorkOrderId);
    }

    [Fact]
    public async Task GetWorkOrderById_WhenNotFound_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/workorders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrder_AsManager_Returns201WithWorkOrder()
    {
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"CWO-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(
            email: $"cwo-{id}@test.com",
            vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.Create(name: $"CWOTask-{id}").Value;

        context.Customers.Add(customer);
        context.Employees.Add(labor);
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);

        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var shopTz = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var shopNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, shopTz);
        var tomorrowAt10 = shopNow.Date.AddDays(1).AddHours(10);
        var startAt = new DateTimeOffset(tomorrowAt10, shopTz.GetUtcOffset(tomorrowAt10));

        var (response, body) = await client.PostAsync<CreateWorkOrderRequest, WorkOrderDto>(
            "/api/v1/workorders",
            new CreateWorkOrderRequest(
                Spot.A,
                vehicle.Id,
                startAt,
                [repairTask.Id],
                labor.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.WorkOrderId);
    }

    [Fact]
    public async Task CreateWorkOrder_AsLabor_Returns403()
    {
        var client = await CreateAuthenticatedClientAsync(Role.Labor);

        var response = await client.PostAsync(
            "/api/v1/workorders",
            new CreateWorkOrderRequest(
                Spot.B,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddHours(1),
                [Guid.NewGuid()],
                Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrder_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/workorders",
            new CreateWorkOrderRequest(
                Spot.C,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddHours(1),
                [Guid.NewGuid()],
                Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_AsManager_WhenExists_Returns204()
    {
        var seeded = await SeedWorkOrderAsync(Spot.D);
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.DeleteAsync($"/api/v1/workorders/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_WhenNotFound_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.DeleteAsync($"/api/v1/workorders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
