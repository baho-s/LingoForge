using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Repositories;

public interface IWordRepository
{
    Task<Word?> GetByIdAsync(WordId id, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByOwnerAsync(UserId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByOwnerPaginatedAsync(UserId ownerId, int skip, int take, CancellationToken ct = default);
    Task<int> GetTotalCountByOwnerAsync(UserId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByFieldPaginatedAsync(UserId ownerId, string field, int skip, int take, CancellationToken ct = default);
    Task<int> GetTotalCountByFieldAsync(UserId ownerId, string field, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByOwnerAndFieldAsync(UserId ownerId, string field, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetWordsWithoutSentenceAsync(UserId ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByOwnerOrderedByDifficultyAsync(UserId ownerId, CancellationToken ct = default);
    
    /// <summary>
    /// Pratik için optimize edilmiş kelimeleri çeker.
    /// Priority sırası:
    /// 1. Overdue (NextReviewAt <= now) - Tekrar zamanı geçmiş
    /// 2. Never reviewed (LastReviewedAt == null) - Hiç yapılmamış
    /// 3. Due soon (NextReviewAt <= now + 1 day) - Yakında tekrar gerekli
    /// 4. Hard (EaseFactor ASC) - Zor olanlar
    /// 5. Random - Diğerleri
    /// 
    /// limit * 2 buffer dönerek, AI Sentence filtresi memory'de yapılabilir.
    /// </summary>
    Task<IReadOnlyList<Word>> GetWordsForPracticeAsync(UserId ownerId, int limit, CancellationToken ct = default);
    
    /// <summary>
    /// Review session için optimize edilmiş query.
    /// NextReviewAt <= today olan kelimeleri dönerek, memory'de filtering'i ortadan kaldırır.
    /// Exclude edilen kelimeleri de göz önüne alır.
    /// ✅ N+1 QUERY ÇÖZÜMÜ: Database'de filtering yapılır
    /// ✅ EF Core SQL Translation: Guid'ler direkt Value Object'e konuşturulur
    /// </summary>
    Task<IReadOnlyList<Word>> GetByOwnerForReviewSessionAsync(
        UserId ownerId,
        DateTime today,
        IEnumerable<Guid> excludeWordGuids,
        int limit,
        CancellationToken ct = default);
    
    /// <summary>
    /// Bulk delete all words for a user in a specific field using ExecuteDeleteAsync.
    /// ✅ PERFORMANCE: Direct SQL DELETE - no memory overhead, no change tracking
    /// ✅ IDEAL FOR: Large datasets (1000+ records)
    /// Returns the number of deleted records.
    /// </summary>
    Task<int> BulkDeleteByFieldAsync(UserId ownerId, string field, CancellationToken ct = default);
    
    void Add(Word word);
    void Update(Word word);
    void Delete(Word word);
}
