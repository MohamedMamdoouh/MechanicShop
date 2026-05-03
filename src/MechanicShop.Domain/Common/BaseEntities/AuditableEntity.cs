namespace MechanicShop.Domain.Common.BaseEntities;

public abstract class AuditableEntity : Entity
{
    public string? CreatedBy { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public string? LastModifiedBy { get; private set; }
    public DateTimeOffset LastModifiedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    // Parameterless constructor for EF Core and other ORMs
    protected AuditableEntity() { }
}
