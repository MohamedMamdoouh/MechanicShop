using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Customer.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    List<CreateVehicleCommand> Vehicles) : IRequest<Result<CustomerDto>>;
