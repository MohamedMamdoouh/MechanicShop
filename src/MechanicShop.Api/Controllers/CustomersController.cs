using Asp.Versioning;
using MechanicShop.Api;
using MechanicShop.Application.Features.Customer.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customer.Commands.DeleteCustomer;
using MechanicShop.Application.Features.Customer.Commands.UpdateCustomer;
using MechanicShop.Contracts.Customers;
using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Customer.Queries.GetCustomerById;
using MechanicShop.Application.Features.Customer.Queries.GetCustomers;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CustomersController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get all customers")]
    [EndpointDescription("Retrieves a list of all customers in the system.")]
    [EndpointName("GetCustomers")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var query = new GetCustomersQuery();
        var result = await sender.Send(query, ct);

        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}", Name = "GetCustomerById")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get customer by ID")]
    [EndpointDescription("Retrieves a customer by their unique identifier.")]
    [EndpointName("GetCustomerById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await sender.Send(query, ct);

        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Create a new customer")]
    [EndpointDescription("Creates a new customer in the system.")]
    [EndpointName("CreateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var command = new CreateCustomerCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            [.. request.Vehicles
                .Select(v => new CreateVehicleCommand(v.Make, v.Model, v.Year, v.LicensePlate))]);

        var result = await sender.Send(command, ct);

        return result.Match(
            customer => CreatedAtRoute("GetCustomerById", new { version = "1.0", id = customer.Id }, customer),
            Problem);
    }

    [HttpPut]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update a customer")]
    [EndpointDescription("Updates an existing customer's information.")]
    [EndpointName("UpdateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update([FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var command = new UpdateCustomerCommand(
            request.CustomerId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command, ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Delete a customer")]
    [EndpointDescription("Deletes a customer from the system. Blocked if the customer has active or scheduled work orders.")]
    [EndpointName("DeleteCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await sender.Send(command, ct);

        return result.Match(_ => NoContent(), Problem);
    }
}