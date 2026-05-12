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

    public async Task<IReadOnlyList<Word>> GetWordsWithoutSentenceAsync(UserId ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Words
            .Where(word => word.OwnerId == ownerId && word.AiSentence == null)
            .ToListAsync(ct);
    }

    public Task<Word?> GetWordOfTheDayAsync(UserId ownerId, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = date.ToDateTime(TimeOnly.MaxValue);

        return _dbContext.Words
            .Where(w => w.OwnerId == ownerId)
            .OrderBy(w => w.Review.EaseFactor) // En zorlanýlan kelimeler en üste
            .FirstOrDefaultAsync(ct);
    }

    public void Add(Word word)
    {
        _dbContext.Words.Add(word);
    }

    public void Update(Word word)
    {
        _dbContext.Words.Update(word);
    }
}
