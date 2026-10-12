using Microsoft.EntityFrameworkCore;
using BeeZillion.Domain.Aggregates.UserAggregate;
using BeeZillion.Domain.Repositories;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, ct);
    }

    public async Task<bool> TryConsumeGuestAiCallsAsync(UserId id, int count, int limit, CancellationToken ct = default)
    {
        var updated = await _dbContext.Users
            .Where(user => user.Id == id && user.IsGuest && user.AiUsageCount + count <= limit)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(user => user.AiUsageCount, user => user.AiUsageCount + count),
                ct);

        return updated == 1;
    }

    public void Add(User user)
    {
        _dbContext.Users.Add(user);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }
}

