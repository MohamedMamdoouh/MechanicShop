using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
namespace MechanicShop.Tests.Common.RepairTasks;

public static class PartFactory
{
    public static Result<Part> Create(
        string? name = null,
        decimal? cost = null,
        int? quantity = null)
    {
        return Part.Create(
            name ?? "Test Part",
            cost ?? 100m,
            quantity ?? 1);
    }
}