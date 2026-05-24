using BeeZillion.Domain.Aggregates.UserAggregate;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    void Add(User user);
    void Update(User user);
}

