using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VocabApp.Domain.Entities;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Infrastructure.Persistence.Configurations;

public sealed class UserVocabularyProgressConfiguration : IEntityTypeConfiguration<UserVocabularyProgress>
{
    public void Configure(EntityTypeBuilder<UserVocabularyProgress> builder)
    {
        var progressIdConverter = new ValueConverter<UserVocabularyProgressId, Guid>(
            id => id.Value,
            value => new UserVocabularyProgressId(value));

        var userIdConverter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            value => new UserId(value));

        var wordIdConverter = new ValueConverter<WordId, Guid>(
            id => id.Value,
            value => new WordId(value));

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(progressIdConverter)
            .ValueGeneratedNever();

        // Composite unique constraint: (UserId, WordId)
        builder.HasIndex(p => new { p.UserId, p.WordId })
            .IsUnique();

        builder.Property(p => p.UserId)
            .HasConversion(userIdConverter)
            .IsRequired();

        builder.Property(p => p.WordId)
            .HasConversion(wordIdConverter)
            .IsRequired();

        builder.Property(p => p.TotalAttempts)
            .IsRequired();

        builder.Property(p => p.CorrectAttempts)
            .IsRequired();

        builder.Property(p => p.AverageTimeTakenMs)
            .IsRequired();

        builder.Property(p => p.MinTimeTakenMs)
            .IsRequired();

        builder.Property(p => p.MaxTimeTakenMs)
            .IsRequired();

        builder.Property(p => p.LastSelectedAt);

        builder.Property(p => p.ConsecutiveSelections)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(p => new { p.UserId, p.LastSelectedAt });
        builder.HasIndex(p => new { p.UserId, p.UpdatedAt });
    }
}
