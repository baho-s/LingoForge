using BeeZillion.Domain.Entities;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Repositories;

public interface IUserVocabularyProgressRepository
{
    Task<UserVocabularyProgress?> GetByIdAsync(UserVocabularyProgressId id, CancellationToken ct = default);
    
    /// <summary>
    /// Belirli bir kullanıcı ve kelime için progress kaydını getirir.
    /// </summary>
    Task<UserVocabularyProgress?> GetByUserAndWordAsync(UserId userId, WordId wordId, CancellationToken ct = default);
    
    /// <summary>
    /// Kullanıcının tüm kelime ilerleme kayıtlarını getirir.
    /// </summary>
    Task<IReadOnlyList<UserVocabularyProgress>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    
    /// <summary>
    /// Kullanıcı ve kelime için progress kaydını getirir veya oluşturur.
    /// </summary>
    Task<UserVocabularyProgress> GetOrCreateAsync(UserId userId, WordId wordId, CancellationToken ct = default);
    
    void Add(UserVocabularyProgress progress);
    void Update(UserVocabularyProgress progress);
    void Delete(UserVocabularyProgress progress);
}

