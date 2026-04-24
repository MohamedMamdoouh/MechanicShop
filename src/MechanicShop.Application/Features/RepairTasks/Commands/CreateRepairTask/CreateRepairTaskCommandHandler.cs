using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandHandler(
    IAppDbContext context,
    ILogger<CreateRepairTaskCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
    public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim().Replace(" ", "");

        var isNameExist = await context.RepairTasks.AnyAsync(
          p => p.Name.Trim().Replace(" ", "") == normalizedName, cancellationToken);

        if (isNameExist)
        {
            logger.LogWarning("Repair task with the same name already exists: {Name}", request.Name);
            return RepairTaskErrors.DuplicateName;
        }

        var repairTaskResult = RepairTask.Create(
            request.Name,
            request.LaborCost,
            request.RepairDurationMinutes,
            []
        );

        if (!repairTaskResult.IsSuccess)
        {
            logger.LogWarning("Failed to create repair task. Error: {Error}", repairTaskResult.TopError);
            return repairTaskResult.Errors.ToList();
        }

        var repairTask = repairTaskResult.Value;

        List<Part> parts = new(request.Parts.Count);

        foreach (var partDto in request.Parts)
        {
            var partResult = Part.Create(partDto.Name, partDto.Cost, partDto.Quantity);

            if (!partResult.IsSuccess)
            {
                logger.LogWarning("Failed to create part. Error: {Error}", partResult.TopError);
                return partResult.Errors.ToList();
            }

            parts.Add(partResult.Value);
        }

        if (parts.Count == 0)
        {
            logger.LogInformation("No valid parts to attach to the repair task.");
            return RepairTaskErrors.PartsRequired;
        }

        var upsertResult = repairTask.UpsertParts(parts);

        if (!upsertResult.IsSuccess)
        {
            logger.LogWarning("Failed to attach parts to repair task. Error: {Error}", upsertResult.TopError);
            return upsertResult.Errors.ToList();
        }

        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(CacheTags.RepairTasks, cancellationToken);

        logger.LogInformation("Repair task with ID {RepairTaskId} created.", repairTask.Id);

        return repairTask.ToDto();
    }
}