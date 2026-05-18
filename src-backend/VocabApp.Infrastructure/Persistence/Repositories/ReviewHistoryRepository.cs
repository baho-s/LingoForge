using Microsoft.EntityFrameworkCore;
using VocabApp.Domain.Entities;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Infrastructure.Persistence.Repositories;

public sealed class ReviewHistoryRepository : IReviewHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public ReviewHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReviewHistory>> GetByUserAsync(UserId userId, CancellationToken ct = default)
    {
        return await _dbContext.ReviewHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ReviewedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReviewHistory>> GetByUserAndWordAsync(UserId userId, WordId wordId, CancellationToken ct = default)
    {
        return await _dbContext.ReviewHistories
            .Where(h => h.UserId == userId && h.WordId == wordId)
            .OrderByDescending(h => h.ReviewedAt)
            .ToListAsync(ct);
    }

    public void Add(ReviewHistory history)
    {
        _dbContext.ReviewHistories.Add(history);
    }
}
