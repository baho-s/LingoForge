using BeeZillion.Domain.Common;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Events;

public sealed record UserStreakUpdatedEvent(UserId UserId, int NewStreak) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

