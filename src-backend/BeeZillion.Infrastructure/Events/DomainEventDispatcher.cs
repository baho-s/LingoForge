using MediatR;
using BeeZillion.Application.Common.Events;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Common;

namespace BeeZillion.Infrastructure.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            if (notification is null)
            {
                continue;
            }

            await _publisher.Publish((INotification)notification, ct);
        }
    }
}

