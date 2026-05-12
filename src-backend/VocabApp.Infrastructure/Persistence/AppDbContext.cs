using Microsoft.EntityFrameworkCore;
using VocabApp.Domain.Aggregates.UserAggregate;
using VocabApp.Domain.Aggregates.WordAggregate;

namespace VocabApp.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Word> Words => Set<Word>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
