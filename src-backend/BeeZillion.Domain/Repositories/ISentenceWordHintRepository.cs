using BeeZillion.Domain.Entities;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Repositories;

public interface ISentenceWordHintRepository
{
    Task<SentenceWordHint?> GetAsync(
        WordId wordId,
        string sentenceHash,
        string normalizedTerm,
        CancellationToken ct = default);

    void Add(SentenceWordHint hint);
}
