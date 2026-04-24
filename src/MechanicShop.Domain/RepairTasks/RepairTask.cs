using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enum;
using MechanicShop.Domain.RepairTasks.Parts;
namespace MechanicShop.Domain.RepairTasks;

public sealed class RepairTask : AuditableEntity
{
    public string Name { get; private set; } = default!;
    public decimal LaborCost { get; private set; }
    public RepairDurationMinutes EstimatedRepairDurationMinutes { get; private set; }

    private readonly List<Part> _parts = [];
    public IReadOnlyCollection<Part> Parts => _parts.AsReadOnly();

    public decimal TotalPartsCost => _parts.Sum(p => p.Cost * p.Quantity);
    public decimal TotalCost => LaborCost + TotalPartsCost;

    // Private constructor for EF Core
    private RepairTask() { }

    private RepairTask(
        string name,
        decimal laborCost,
        RepairDurationMinutes estimatedRepairDurationMinutes,
        List<Part> parts)
    {
        Name = name.Trim();
        LaborCost = laborCost;
        EstimatedRepairDurationMinutes = estimatedRepairDurationMinutes;
        _parts = parts ?? [];
    }

    public static Result<RepairTask> Create(
        string name,
        decimal laborCost,
        RepairDurationMinutes estimatedRepairDurationMinutes,
        List<Part> parts
        )
    {
        var errors = Validate(name, laborCost, estimatedRepairDurationMinutes);

        if (errors.Count > 0)
        {
            return errors;
        }

        return new RepairTask(name, laborCost, estimatedRepairDurationMinutes, parts);
    }

    public Result<Updated> Update(
        string name,
        decimal laborCost,
        RepairDurationMinutes repairDuration)
    {
        var errors = Validate(name, laborCost, repairDuration);

        if (errors.Count > 0)
        {
            return errors;
        }

        Name = name!.Trim();
        LaborCost = laborCost;
        EstimatedRepairDurationMinutes = repairDuration;

        return Result.Updated;
    }

    private static List<Error> Validate(
        string name,
        decimal laborCost,
        RepairDurationMinutes duration)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(RepairTaskErrors.NameRequired);
        }

        if (laborCost <= 0)
        {
            errors.Add(RepairTaskErrors.LaborCostInvalid);
        }

        if (!System.Enum.IsDefined(duration))
        {
            errors.Add(RepairTaskErrors.DurationInvalid);
        }

        return errors;
    }

    public Result<Updated> UpsertParts(List<Part> parts)
    {
        if (parts == null || parts.Count == 0)
            return RepairTaskErrors.PartsRequired;

        if (parts.Exists(p => p is null))
            return RepairTaskErrors.PartsRequired;

        var hasDuplicateNames = parts
            .GroupBy(p => p.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1);

        if (hasDuplicateNames)
            return RepairTaskErrors.DuplicatePartName;

        var incomingIds = parts.Select(p => p.Id).ToHashSet();
        var errors = new List<Error>();

        foreach (var part in parts)
            ProcessPartUpsert(part, incomingIds, errors);

        if (errors.Count > 0)
            return errors;

        return Result.Updated;
    }

    public Result<Updated> ReplaceParts(List<Part> parts)
    {
        if (parts == null || parts.Count == 0)
            return RepairTaskErrors.PartsRequired;

        if (parts.Exists(p => p is null))
            return RepairTaskErrors.PartsRequired;

        var hasDuplicateNames = parts
            .GroupBy(p => p.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1);

        if (hasDuplicateNames)
            return RepairTaskErrors.DuplicatePartName;

        _parts.Clear();
        _parts.AddRange(parts);

        return Result.Updated;
    }

    private void ProcessPartUpsert(Part part, HashSet<Guid> incomingIds, List<Error> errors)
    {
        if (HasPartNameConflict(part, incomingIds))
        {
            errors.Add(RepairTaskErrors.DuplicatePartName);
            return;
        }

        var existingPart = _parts.Find(p => p.Id == part.Id);

        if (existingPart is not null)
        {
            var updateResult = existingPart.Update(part.Name, part.Cost, part.Quantity);
            if (!updateResult.IsSuccess)
                errors.AddRange(updateResult.Errors);
        }
        else
        {
            _parts.Add(part);
        }
    }

    private bool HasPartNameConflict(Part part, HashSet<Guid> incomingIds) =>
        _parts.Exists(p =>
            !incomingIds.Contains(p.Id) &&
            string.Equals(p.Name.Trim(), part.Name.Trim(), StringComparison.OrdinalIgnoreCase));
}