using Microsoft.EntityFrameworkCore;
using VocabApp.Domain.Aggregates.UserAggregate;
using VocabApp.Domain.Repositories;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Infrastructure.Persistence.Repositories;

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

    public void Add(User user)
    {
        _dbContext.Users.Add(user);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }
}
