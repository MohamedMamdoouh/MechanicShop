using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Domain.Employees;
namespace MechanicShop.Application.Features.Labor.Mappers;

public static class LaborMapper
{
    public static LaborDto ToDto(this Employee employee)
    {
        return new LaborDto
        {
            Id = employee.Id,
            Name = employee.FullName,
        };
    }

    public static List<LaborDto> ToDtos(this IEnumerable<Employee> employees)
    {
        return [.. employees.Select(e => e.ToDto())];
    }
}