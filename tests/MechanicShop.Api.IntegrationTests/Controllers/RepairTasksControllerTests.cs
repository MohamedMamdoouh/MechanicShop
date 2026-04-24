using System.Net;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Contracts.RepairTasks;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks.Enum;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebFactoryCollection.CollectionName)]
public class RepairTasksControllerTests(WebFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetRepairTasks_WhenAuthenticated_Returns200WithList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<List<RepairTaskDto>>("/api/v1/repairtasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetRepairTasks_WhenUnauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/repairtasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTaskById_WhenExists_Returns200WithRepairTask()
    {
        var seeded = await SeedRepairTaskAsync();
        var client = await CreateAuthenticatedClientAsync();

        var (response, body) = await client.GetAsync<RepairTaskDto>($"/api/v1/repairtasks/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(seeded.Id, body.RepairTaskId);
    }

    [Fact]
    public async Task GetRepairTaskById_WhenNotFound_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/repairtasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_AsManager_Returns201WithRepairTask()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var (response, body) = await client.PostAsync<CreateRepairTaskRequest, RepairTaskDto>(
            "/api/v1/repairtasks",
            new CreateRepairTaskRequest(
                $"Brake Service {id}",
                150m,
                RepairDurationMinutes.Min60,
                [new CreateRepairTaskPartRequest("Brake Pad", 40m, 2)]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.RepairTaskId);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateRepairTask_AsLabor_Returns403()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = await CreateAuthenticatedClientAsync(Role.Labor);

        var response = await client.PostAsync(
            "/api/v1/repairtasks",
            new CreateRepairTaskRequest(
                $"Oil Change {id}",
                50m,
                RepairDurationMinutes.Min30,
                [new CreateRepairTaskPartRequest("Oil Filter", 10m, 1)]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateRepairTask_WhenUnauthenticated_Returns401()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var client = CreateUnauthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/repairtasks",
            new CreateRepairTaskRequest(
                $"Tyre Rotation {id}",
                30m,
                RepairDurationMinutes.Min30,
                []));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_AsManager_WhenExists_Returns204()
    {
        var seeded = await SeedRepairTaskAsync();
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.DeleteAsync($"/api/v1/repairtasks/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_WhenNotFound_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync(Role.Manager);

        var response = await client.DeleteAsync($"/api/v1/repairtasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_AsLabor_Returns403()
    {
        var seeded = await SeedRepairTaskAsync();
        var client = await CreateAuthenticatedClientAsync(Role.Labor);

        var response = await client.DeleteAsync($"/api/v1/repairtasks/{seeded.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
