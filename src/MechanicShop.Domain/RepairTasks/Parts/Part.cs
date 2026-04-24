using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.RepairTasks.Parts;

public sealed class Part : AuditableEntity
{
    public string Name { get; private set; } = default!;
    public decimal Cost { get; private set; }
    public int Quantity { get; private set; }

    // EF Core requires a parameterless constructor for materialization.
    private Part() { }

    private Part(string name, decimal cost, int quantity)
    {
        Name = name;
        Cost = cost;
        Quantity = quantity;
    }

    public static Result<Part> Create(string name, decimal cost, int quantity)
    {
        var errors = Validate(name, cost, quantity);

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Part(name.Trim(), cost, quantity);
    }

    public Result<Updated> Update(string name, decimal cost, int quantity)
    {
        var errors = Validate(name, cost, quantity);

        if (errors.Count > 0)
        {
            return errors;
        }

        Name = name.Trim();
        Cost = cost;
        Quantity = quantity;

        return Result.Updated;
    }

    private static List<Error> Validate(string name, decimal cost, int quantity)
    {
        var errors = new List<Error>();

        if (quantity <= 0)
        {
            errors.Add(PartErrors.QuantityInvalid);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(PartErrors.NameRequired);
        }

        if (cost <= 0)
        {
            errors.Add(PartErrors.CostInvalid);
        }

        return errors;
    }
}