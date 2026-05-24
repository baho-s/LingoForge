using BeeZillion.Domain.Aggregates.PredefinedWordAggregate;

namespace BeeZillion.Domain.Repositories;

public interface IPredefinedWordRepository
{
    Task AddAsync(PredefinedWord predefinedWord, CancellationToken cancellationToken = default);
    Task<PredefinedWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetDistinctFieldsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PredefinedWord>> GetByFieldAsync(string field, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PredefinedWord>> GetByFieldAndCategoryAsync(string field, string category, CancellationToken cancellationToken = default);
    Task<int> CountByFieldAsync(string field, CancellationToken cancellationToken = default);
    Task<bool> ExistsByFieldAndOriginalAsync(string field, string original, CancellationToken cancellationToken = default);
}

