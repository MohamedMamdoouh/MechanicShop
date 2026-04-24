using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Application.Features.RepairTasks.Mappers;
namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public sealed class GetRepairTasksQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRepairTasksQuery, Result<List<RepairTaskDto>>>
{
    public async Task<Result<List<RepairTaskDto>>> Handle(GetRepairTasksQuery request, CancellationToken cancellationToken)
    {
        var repairTasks = await context.RepairTasks.AsNoTracking()
        .Include(rt => rt.Parts)
        .ToListAsync(cancellationToken);

        return repairTasks.ToDto();
    }
}