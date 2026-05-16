using VocabApp.Domain.Common;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Events;

public sealed record WordsDeletedFromFieldEvent(UserId UserId, string Field, int Count) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
