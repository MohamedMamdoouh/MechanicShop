using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enum;
using MechanicShop.Domain.RepairTasks.Parts;
namespace MechanicShop.Tests.Common.RepairTasks;

public static class RepairTaskFactory
{
    public static Result<RepairTask> Create(
        string? name = null,
        decimal? laborCost = null,
        RepairDurationMinutes? estimatedRepairDurationMinutes = null,
        List<Part>? parts = null)
    {
        return RepairTask.Create(
            name ?? "Test Repair Task",
            laborCost ?? 100m,
            estimatedRepairDurationMinutes ?? RepairDurationMinutes.Min60,
            parts ?? [PartFactory.Create().Value]);
    }
}

