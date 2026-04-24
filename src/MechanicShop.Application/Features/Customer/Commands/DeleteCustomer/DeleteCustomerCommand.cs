using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Customer.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result<Deleted>>;
