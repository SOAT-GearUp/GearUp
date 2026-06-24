using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AdicionarDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void LimparDomainEvents()
    {
        _domainEvents.Clear();
    }
}
