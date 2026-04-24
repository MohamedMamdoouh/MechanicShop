using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Features.Identity.Queries;

public sealed record GetUserInfoQuery(string UserId) : IRequest<Result<AppUserDto>>;