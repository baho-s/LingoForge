using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VocabApp.Domain.Aggregates.WordAggregate;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Infrastructure.Persistence.Configurations;

public sealed class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        var wordIdConverter = new ValueConverter<WordId, Guid>(
            id => id.Value,
            value => new WordId(value));

        var userIdConverter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            value => new UserId(value));

        builder.HasKey(word => word.Id);
        builder.Property(word => word.Id)
            .HasConversion(wordIdConverter)
            .ValueGeneratedNever();

        builder.Property(word => word.OwnerId)
            .HasConversion(userIdConverter)
            .IsRequired();

        builder.Property(word => word.Original)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(word => word.Translation)
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(word => word.AiSentence)
            .HasMaxLength(1000);

        builder.Property(word => word.CreatedAt)
            .IsRequired();

        builder.OwnsOne(word => word.Review, review =>
        {
            review.Property(r => r.IntervalDays).HasColumnName("ReviewIntervalDays");
            review.Property(r => r.EaseFactor).HasColumnName("ReviewEaseFactor");
            review.Property(r => r.Repetitions).HasColumnName("ReviewRepetitions");
            review.Property(r => r.NextReviewAt).HasColumnName("ReviewNextReviewAt");
            review.Property(r => r.LastReviewedAt).HasColumnName("ReviewLastReviewedAt");
        });

        builder.HasIndex(word => new { word.OwnerId, word.CreatedAt });
    }
}
