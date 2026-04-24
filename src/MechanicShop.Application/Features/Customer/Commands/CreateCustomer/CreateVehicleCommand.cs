using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Customer.Commands.CreateCustomer;

public sealed record CreateVehicleCommand(
    string Make,
    string Model,
    int Year,
    string LicensePlate) : IRequest<Result<VehicleDto>>;