using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Application.Features.Labor.Queries;

public sealed class GetLaborsQueryHandler(IAppDbContext context) : IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
    public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
    {
        var labors = await context.Employees.AsNoTracking()
            .Where(x => x.Role == Role.Labor)
            .ToListAsync(cancellationToken);

        return labors.ToDtos();
    }
}