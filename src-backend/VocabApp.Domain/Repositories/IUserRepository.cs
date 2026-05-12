using VocabApp.Domain.Aggregates.UserAggregate;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    void Add(User user);
    void Update(User user);
}
