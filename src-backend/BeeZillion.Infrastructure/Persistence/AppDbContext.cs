using Microsoft.EntityFrameworkCore;
using BeeZillion.Domain.Aggregates.PredefinedWordAggregate;
using BeeZillion.Domain.Aggregates.UserAggregate;
using BeeZillion.Domain.Aggregates.WordAggregate;
using BeeZillion.Domain.Entities;

namespace BeeZillion.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Word> Words => Set<Word>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PredefinedWord> PredefinedWords => Set<PredefinedWord>();
    public DbSet<UserVocabularyProgress> UserVocabularyProgresses => Set<UserVocabularyProgress>();
    public DbSet<ReviewHistory> ReviewHistories => Set<ReviewHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

