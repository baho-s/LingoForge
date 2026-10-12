using Microsoft.EntityFrameworkCore;
using BeeZillion.Domain.Entities;
using BeeZillion.Domain.Repositories;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Repositories;

public sealed class SentenceWordHintRepository : ISentenceWordHintRepository
{
    private readonly AppDbContext _dbContext;

    public SentenceWordHintRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SentenceWordHint?> GetAsync(
        WordId wordId,
        string sentenceHash,
        string normalizedTerm,
        CancellationToken ct = default)
    {
        return _dbContext.SentenceWordHints.FirstOrDefaultAsync(
            hint => hint.WordId == wordId
                && hint.SentenceHash == sentenceHash
                && hint.NormalizedTerm == normalizedTerm,
            ct);
    }

    public void Add(SentenceWordHint hint)
    {
        _dbContext.SentenceWordHints.Add(hint);
    }
}
