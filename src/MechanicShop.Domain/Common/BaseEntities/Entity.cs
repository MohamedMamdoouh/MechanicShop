using System.ComponentModel.DataAnnotations.Schema;
namespace MechanicShop.Domain.Common.BaseEntities;

public abstract class Entity
{
    public Guid Id { get; protected init; }

    private readonly List<DomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Parameterless constructor for EF Core and other ORMs
    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

