using Microsoft.EntityFrameworkCore;
using BeeZillion.Domain.Aggregates.PredefinedWordAggregate;
using BeeZillion.Domain.Repositories;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Repositories;

public sealed class PredefinedWordRepository : IPredefinedWordRepository
{
    private readonly AppDbContext _context;

    public PredefinedWordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PredefinedWord predefinedWord, CancellationToken cancellationToken = default)
    {
        await _context.PredefinedWords.AddAsync(predefinedWord, cancellationToken);
    }

    public async Task<PredefinedWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PredefinedWords.FirstOrDefaultAsync(pw => pw.Id.Value == id, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctFieldsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PredefinedWords
            .Where(pw => pw.IsActive)
            .Select(pw => pw.Field)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PredefinedWord>> GetByFieldAsync(string field, CancellationToken cancellationToken = default)
    {
        return await _context.PredefinedWords
            .Where(pw => pw.Field == field && pw.IsActive)
            .OrderBy(pw => pw.Category)
            .ThenBy(pw => pw.Original)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PredefinedWord>> GetByFieldAndCategoryAsync(
        string field,
        string category,
        CancellationToken cancellationToken = default)
    {
        return await _context.PredefinedWords
            .Where(pw => pw.Field == field && pw.Category == category && pw.IsActive)
            .OrderBy(pw => pw.Original)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByFieldAsync(string field, CancellationToken cancellationToken = default)
    {
        return await _context.PredefinedWords
            .CountAsync(pw => pw.Field == field && pw.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByFieldAndOriginalAsync(string field, string original, CancellationToken cancellationToken = default)
    {
        return await _context.PredefinedWords
            .AnyAsync(pw => pw.Field == field && pw.Original == original && pw.IsActive, cancellationToken);
    }
}

