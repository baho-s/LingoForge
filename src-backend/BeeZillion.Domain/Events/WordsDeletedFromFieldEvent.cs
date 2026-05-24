using BeeZillion.Domain.Common;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Events;

public sealed record WordsDeletedFromFieldEvent(UserId UserId, string Field, int Count) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

