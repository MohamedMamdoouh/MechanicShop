using Asp.Versioning;
using MechanicShop.Api;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MechanicShop.Contracts.Invoices;
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
public sealed class InvoicesController(ISender sender) : ApiController
{
    [HttpGet("{id:guid}", Name = "GetInvoiceById")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get invoice by ID")]
    [EndpointDescription("Retrieves the details of a single invoice by its identifier.")]
    [EndpointName("GetInvoiceById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}/pdf")]
    [EnableRateLimiting(RateLimitPolicies.PdfExport)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Download invoice PDF")]
    [EndpointDescription("Generates and downloads the invoice as a PDF file.")]
    [EndpointName("GetInvoicePdf")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken ct)
    {
        var query = new GetInvoicePdfQuery(id);
        var result = await sender.Send(query, ct);
        return result.Match(
            dto => File(dto.PdfContent, dto.ContentType, dto.FileName),
            Problem);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Issue invoice")]
    [EndpointDescription("Issues an invoice for a completed work order.")]
    [EndpointName("IssueInvoice")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Issue([FromBody] IssueInvoiceRequest request, CancellationToken ct)
    {
        var command = new IssueInvoiceCommand(request.WorkOrderId);
        var result = await sender.Send(command, ct);
        return result.Match(
            invoice => CreatedAtRoute("GetInvoiceById", new { version = "1.0", id = invoice.InvoiceId }, invoice),
            Problem);
    }

    [HttpPost("{id:guid}/settle")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Settle invoice")]
    [EndpointDescription("Marks an invoice as paid, settling all outstanding balances.")]
    [EndpointName("SettleInvoice")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Settle(Guid id, CancellationToken ct)
    {
        var command = new SettleInvoiceCommand(id);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }
}