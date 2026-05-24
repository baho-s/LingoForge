using BeeZillion.Domain.Common;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Events;

public sealed record BadgeEarnedEvent(UserId UserId, BadgeType Badge) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

