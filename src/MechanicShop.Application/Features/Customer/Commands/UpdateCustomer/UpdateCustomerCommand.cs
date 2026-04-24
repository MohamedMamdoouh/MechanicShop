using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Customer.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber) : IRequest<Result<Updated>>;
