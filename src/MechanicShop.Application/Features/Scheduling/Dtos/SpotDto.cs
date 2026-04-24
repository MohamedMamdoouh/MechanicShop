using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed record SpotDto
{
    public Spot Spot { get; init; }
    public List<AvailabilitySlotDto> AvailabilitySlots { get; init; } = [];
}