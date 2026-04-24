using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskMapper
{
    public static PartDto ToDto(this Part part)
    {
        return new PartDto
        {
            PartId = part.Id,
            Name = part.Name,
            Cost = part.Cost,
            Quantity = part.Quantity
        };
    }

    public static List<PartDto> ToDto(this List<Part> parts)
    {
        return [.. parts.Select(p => p.ToDto())];
    }

    public static RepairTaskDto ToDto(this RepairTask repairTask)
    {
        return new RepairTaskDto
        {
            RepairTaskId = repairTask.Id,
            Name = repairTask.Name,
            RepairDurationMinutes = repairTask.EstimatedRepairDurationMinutes,
            LaborCost = repairTask.LaborCost,
            TotalCost = repairTask.TotalCost,
            Parts = repairTask.Parts.ToList().ToDto()
        };
    }

    public static List<RepairTaskDto> ToDto(this List<RepairTask> repairTasks)
    {
        return [.. repairTasks.Select(rt => rt.ToDto())];
    }
}