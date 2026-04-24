namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed record ScheduleDto
{
    public DateOnly Date { get; init; }
    public bool IsDayInPast { get; init; }
    public List<SpotDto> Slots { get; init; } = [];
}