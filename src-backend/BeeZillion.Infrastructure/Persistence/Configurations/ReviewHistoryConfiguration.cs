using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BeeZillion.Domain.Entities;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Configurations;

public sealed class ReviewHistoryConfiguration : IEntityTypeConfiguration<ReviewHistory>
{
    public void Configure(EntityTypeBuilder<ReviewHistory> builder)
    {
        var historyIdConverter = new ValueConverter<ReviewHistoryId, Guid>(
            id => id.Value,
            value => new ReviewHistoryId(value));

        var userIdConverter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            value => new UserId(value));

        var wordIdConverter = new ValueConverter<WordId, Guid>(
            id => id.Value,
            value => new WordId(value));

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasConversion(historyIdConverter)
            .ValueGeneratedNever();

        builder.Property(h => h.UserId)
            .HasConversion(userIdConverter)
            .IsRequired();

        builder.Property(h => h.WordId)
            .HasConversion(wordIdConverter)
            .IsRequired();

        builder.Property(h => h.IsCorrect)
            .IsRequired();

        builder.Property(h => h.Outcome)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.QScore);
        builder.Property(h => h.TimeTakenMs);

        builder.Property(h => h.ReviewedAt)
            .IsRequired();

        builder.Property(h => h.NextReviewAt)
            .IsRequired();

        builder.Property(h => h.IntervalDays)
            .IsRequired();

        builder.Property(h => h.EaseFactor)
            .IsRequired();

        builder.Property(h => h.Repetitions)
            .IsRequired();

        builder.Property(h => h.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.SessionId);

        builder.Property(h => h.ClientVersion)
            .HasMaxLength(50);

        builder.HasIndex(h => new { h.UserId, h.WordId });
        builder.HasIndex(h => new { h.UserId, h.ReviewedAt });
        builder.HasIndex(h => new { h.WordId, h.ReviewedAt });
        builder.HasIndex(h => new { h.UserId, h.Outcome });
    }
}

