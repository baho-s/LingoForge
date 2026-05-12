using MediatR;
using Microsoft.Extensions.Logging;
using VocabApp.Application.Common.Events;
using VocabApp.Domain.Events;

namespace VocabApp.Application.Users.Events;

public sealed class BadgeEarnedEventHandler : INotificationHandler<DomainEventNotification<BadgeEarnedEvent>>
{
    private readonly ILogger<BadgeEarnedEventHandler> _logger;

    public BadgeEarnedEventHandler(ILogger<BadgeEarnedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<BadgeEarnedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation(
            "Badge earned: {Badge} by user {UserId}",
            domainEvent.Badge,
            domainEvent.UserId.Value);

        return Task.CompletedTask;
    }
}
