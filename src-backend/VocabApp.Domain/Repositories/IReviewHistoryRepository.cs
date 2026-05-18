using VocabApp.Domain.Entities;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Repositories;

public interface IReviewHistoryRepository
{
    Task<IReadOnlyList<ReviewHistory>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewHistory>> GetByUserAndWordAsync(UserId userId, WordId wordId, CancellationToken ct = default);
    void Add(ReviewHistory history);
}
