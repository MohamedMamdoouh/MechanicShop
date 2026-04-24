using MechanicShop.Domain.RepairTasks.Enum;
namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed record RepairTaskDto
{
    public Guid RepairTaskId { get; init; }
    public string Name { get; init; } = null!;
    public RepairDurationMinutes RepairDurationMinutes { get; init; }
    public decimal LaborCost { get; init; }
    public decimal TotalCost { get; init; }
    public List<PartDto> Parts { get; init; } = [];
}