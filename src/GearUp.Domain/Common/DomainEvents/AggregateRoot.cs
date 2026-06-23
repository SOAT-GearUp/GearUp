namespace GearUp.Domain.Common.DomainEvents
{
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyCollection<IDomainEvent> DomainEvents =>
            _domainEvents.AsReadOnly();

        protected void AdicionarDomainEvent(
            IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void LimparDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
