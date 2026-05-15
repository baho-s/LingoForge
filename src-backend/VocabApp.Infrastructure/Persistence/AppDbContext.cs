using Microsoft.EntityFrameworkCore;
using VocabApp.Domain.Aggregates.PredefinedWordAggregate;
using VocabApp.Domain.Aggregates.UserAggregate;
using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.Entities;

namespace VocabApp.Infrastructure.Persistence;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
