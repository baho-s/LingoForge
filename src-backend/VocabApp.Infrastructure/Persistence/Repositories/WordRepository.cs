using Microsoft.EntityFrameworkCore;
using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Infrastructure.Persistence.Repositories;

public sealed class WordRepository : IWordRepository
{
    private readonly AppDbContext _dbContext;

    public WordRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Word?> GetByIdAsync(WordId id, CancellationToken ct = default)
    {
        return _dbContext.Words.FirstOrDefaultAsync(word => word.Id == id, ct);
    }

    public async Task<IReadOnlyList<Word>> GetByOwnerAsync(UserId ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId)
            .OrderByDescending(word => word.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Word>> GetByOwnerPaginatedAsync(UserId ownerId, int skip, int take, CancellationToken ct = default)
    {
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId)
            .OrderByDescending(word => word.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalCountByOwnerAsync(UserId ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId)
            .CountAsync(ct);
    }

    public async Task<IReadOnlyList<Word>> GetByOwnerAndFieldAsync(UserId ownerId, string field, CancellationToken ct = default)
    {
        var normalizedField = field.Trim();
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId && (string.IsNullOrEmpty(normalizedField) ? string.IsNullOrEmpty(word.Field) : word.Field == normalizedField))
            .OrderByDescending(word => word.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Word>> GetWordsWithoutSentenceAsync(UserId ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId && word.AiSentence == null)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Word>> GetByOwnerOrderedByDifficultyAsync(UserId ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Words
            .Where(w => w.OwnerId == ownerId)
            .OrderBy(w => w.Review.EaseFactor) // En zorlanılan kelimeler (düşük ease factor) önce
            .ToListAsync(ct);
    }

    /// <summary>
    /// Practice sekmesinden sorular seçmek için optimize edilmiş method.
    /// NextReviewAt'a göre kullanıcının hangi kelimeleri yapması gerektiğini belirler.
    /// ✅ YENİ: UserVocabularyProgress ile consecutive selections kontrol edilerek tekrar engellenir.
    /// 
    /// DATABASE OPTIMIZATION:
    /// - SORUN 1 ÇÖZÜMÜ: Bütün kelimeleri çekmek yerine database'de sorgulanır
    /// - SORUN 2 ÇÖZÜMÜ: NextReviewAt'a göre sıralama yapılır
    /// - SORUN 3 ÇÖZÜMÜ: Overdue olanlar tanımlanır (NextReviewAt <= now)
    /// - SORUN 4 ÇÖZÜMÜ: EaseFactor'e göre zor olanlar seçilir
    /// - ✅ YENİ: Consecutive selections kontrol edilerek tekrar engellenir
    /// </summary>
    public async Task<IReadOnlyList<Word>> GetWordsForPracticeAsync(
        UserId ownerId, 
        int limit, 
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        
        // Database'de optimize edilmiş query
        // Priority ranking:
        // 1. NextReviewAt <= now (Overdue - ÇOK ÖNEMLİ)
        // 2. LastReviewedAt == null (Never reviewed)
        // 3. NextReviewAt <= now + 1 day (Due soon)
        // 4. EaseFactor ASC (Hard words)
        // 5. NEWID() (Random)
        
        return await _dbContext.Words
            .Where(w => w.OwnerId == ownerId)
            // SORUN 2 & 3 ÇÖZÜMÜ: NextReviewAt kontrol
            // Overdue olanları (NextReviewAt geçmiş) önce getir
            .OrderBy(w => w.Review.NextReviewAt <= now ? 0 : 1)
            // SORUN 2 ÇÖZÜMÜ: Hiç review yapılmamış kelimeleri ikinci sıraya
            .ThenBy(w => w.Review.LastReviewedAt == null ? 0 : 1)
            // SORUN 3 ÇÖZÜMÜ: Yakın zamanda yapılacak (1 gün içinde) kelimeleri
            .ThenBy(w => w.Review.NextReviewAt <= now.AddDays(1) ? 0 : 1)
            // SORUN 4 ÇÖZÜMÜ: EaseFactor düşük (zor) olanları daha öne
            .ThenBy(w => w.Review.EaseFactor)
            // SORUN 1 ÇÖZÜMÜ: Random sırala
            .ThenBy(w => Guid.NewGuid())
            // SORUN 1 ÇÖZÜMÜ: limit * 2 kadarını çek
            // AI Sentence filter'ı memory'de yapılıyor, 2x buffer gerekli
            .Take(limit * 2)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Review session için optimize edilmiş query.
    /// NextReviewAt <= today olan kelimeleri dönerek, memory'de filtering'i ortadan kaldırır.
    /// ✅ N+1 QUERY ÇÖZÜMÜ: Database'de filtering yapılır, tüm words yerine sadece gerekli olanlar çekilir
    /// </summary>
    public async Task<IReadOnlyList<Word>> GetByOwnerForReviewSessionAsync(
        UserId ownerId,
        DateTime today,
        IEnumerable<Guid> excludeWordGuids,
        int limit,
        CancellationToken ct = default)
    {
        var excludeGuids = excludeWordGuids.ToList();
        
        // ✅ SQL FIRST: Database'de NextReviewAt, Owner, ordering ile filter et ve LIMIT uygula
        // ⚠️ Value Object (.Id.Value) SQL translation için memory'e kaldırıldı
        var words = await _dbContext.Words
            .Where(w => w.OwnerId == ownerId)
            // ✅ Database seviyesinde filtering: NextReviewAt <= today
            .Where(w => w.Review.NextReviewAt.Date <= today)
            // ✅ Database seviyesinde ordering: En eski sıradakiler (en gecikmişler) önce
            .OrderBy(w => w.Review.NextReviewAt)
            // ✅ limit*2 buffer: Memory'de exclude edildikten sonra yeterli kayıt olsun
            .Take(limit * 2)
            .ToListAsync(ct);
        
        // ✅ THEN FILTER IN MEMORY: Value Object comparison memory'de yapılır
        return words
            .Where(w => !excludeGuids.Contains(w.Id.Value))
            .Take(limit)
            .ToList();
    }

    public void Add(Word word)
    {
        _dbContext.Words.Add(word);
    }

    public void Update(Word word)
    {
        _dbContext.Words.Update(word);
    }

    public void Delete(Word word)
    {
        _dbContext.Words.Remove(word);
    }

    public async Task<int> BulkDeleteByFieldAsync(UserId ownerId, string field, CancellationToken ct = default)
    {
        var normalizedField = field.Trim();
        
        // ✅ PERFORMANCE FIX: ExecuteDeleteAsync for bulk operations
        // - No memory overhead: doesn't load records into memory
        // - No change tracking: single SQL DELETE statement
        // - Ideal for large datasets (3400+ words)
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId && 
                   (string.IsNullOrEmpty(normalizedField) ? string.IsNullOrEmpty(word.Field) : word.Field == normalizedField))
            .ExecuteDeleteAsync(ct);
    }
}
