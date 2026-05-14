using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Repositories;

public interface IWordRepository
{
    Task<Word?> GetByIdAsync(WordId id, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByOwnerAsync(UserId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetWordsWithoutSentenceAsync(UserId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByOwnerOrderedByDifficultyAsync(UserId ownerId, CancellationToken ct = default);
    void Add(Word word);
    void Update(Word word);
}
