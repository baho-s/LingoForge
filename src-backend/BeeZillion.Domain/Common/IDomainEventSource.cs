namespace BeeZillion.Domain.Common;

public interface IDomainEventSource
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

