using VocabApp.Domain.Common;
using VocabApp.Domain.Enums;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Events;

public sealed record BadgeEarnedEvent(UserId UserId, BadgeType Badge) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
