using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskCommandHandler(
    IAppDbContext context,
    ILogger<UpdateRepairTaskCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await context.RepairTasks.Include(rt => rt.Parts)
        .FirstOrDefaultAsync(rt => rt.Id == request.RepairTaskId, cancellationToken);

        if (repairTask is null)
        {
            logger.LogWarning("Repair task with ID {RepairTaskId} not found.", request.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }

        var existingPartsById = repairTask.Parts.ToDictionary(p => p.Id);
        List<Part> parts = new(request.Parts.Count);

        foreach (var partDto in request.Parts)
        {
            if (partDto.PartId != Guid.Empty)
            {
                if (!existingPartsById.TryGetValue(partDto.PartId, out var existingPart))
                {
                    logger.LogWarning(
                        "Part {PartId} not found in repair task {RepairTaskId}.",
                        partDto.PartId,
                        request.RepairTaskId);
                    return ApplicationErrors.PartNotFound;
                }

                var updatePartResult = existingPart.Update(partDto.Name, partDto.Cost, partDto.Quantity);

                if (!updatePartResult.IsSuccess)
                {
                    logger.LogWarning("Failed to update part. Error: {Error}", updatePartResult.TopError);
                    return updatePartResult.Errors.ToList();
                }

                parts.Add(existingPart);
                continue;
            }

            var createPartResult = Part.Create(partDto.Name, partDto.Cost, partDto.Quantity);

            if (!createPartResult.IsSuccess)
            {
                logger.LogWarning("Failed to create part. Error: {Error}", createPartResult.TopError);
                return createPartResult.Errors.ToList();
            }

            parts.Add(createPartResult.Value);
        }

        var updateResult = repairTask.Update(
            request.Name,
            request.LaborCost,
            request.RepairDurationMinutes);

        if (!updateResult.IsSuccess)
        {
            logger.LogWarning("Failed to update repair task. Error: {Error}", updateResult.TopError);
            return updateResult.Errors.ToList();
        }

        var replacePartsResult = repairTask.ReplaceParts(parts);

        if (!replacePartsResult.IsSuccess)
        {
            logger.LogWarning("Failed to replace parts. Error: {Error}", replacePartsResult.TopError);
            return replacePartsResult.Errors.ToList();
        }

        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.RepairTasks, cancellationToken);

        return Result.Updated;
    }
}