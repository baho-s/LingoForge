using Microsoft.EntityFrameworkCore;
using BeeZillion.Domain.Entities;
using BeeZillion.Domain.Repositories;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Repositories;

public sealed class UserVocabularyProgressRepository : IUserVocabularyProgressRepository
{
    private readonly AppDbContext _dbContext;

    public UserVocabularyProgressRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserVocabularyProgress?> GetByIdAsync(UserVocabularyProgressId id, CancellationToken ct = default)
    {
        return _dbContext.UserVocabularyProgresses.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public Task<UserVocabularyProgress?> GetByUserAndWordAsync(UserId userId, WordId wordId, CancellationToken ct = default)
    {
        return _dbContext.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == wordId, ct);
    }

    public async Task<IReadOnlyList<UserVocabularyProgress>> GetByUserAsync(UserId userId, CancellationToken ct = default)
    {
        return await _dbContext.UserVocabularyProgresses
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<UserVocabularyProgress> GetOrCreateAsync(UserId userId, WordId wordId, CancellationToken ct = default)
    {
        var existing = await GetByUserAndWordAsync(userId, wordId, ct);
        
        if (existing is not null)
        {
            return existing;
        }

        var newProgress = UserVocabularyProgress.Create(userId, wordId);
        _dbContext.UserVocabularyProgresses.Add(newProgress);
        
        // SaveChanges'i repository'de çağırmıyoruz; UnitOfWork'a bırakıyoruz
        
        return newProgress;
    }

    public void Add(UserVocabularyProgress progress)
    {
        _dbContext.UserVocabularyProgresses.Add(progress);
    }

    public void Update(UserVocabularyProgress progress)
    {
        _dbContext.UserVocabularyProgresses.Update(progress);
    }

    public void Delete(UserVocabularyProgress progress)
    {
        _dbContext.UserVocabularyProgresses.Remove(progress);
    }
}

