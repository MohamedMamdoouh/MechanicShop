namespace MechanicShop.Application.Features.Labor.Dtos;

public sealed record LaborDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
}