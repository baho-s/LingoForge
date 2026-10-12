using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BeeZillion.Domain.Aggregates.WordAggregate;
using BeeZillion.Domain.Entities;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Configurations;

public sealed class SentenceWordHintConfiguration : IEntityTypeConfiguration<SentenceWordHint>
{
    public void Configure(EntityTypeBuilder<SentenceWordHint> builder)
    {
        var wordIdConverter = new ValueConverter<WordId, Guid>(
            id => id.Value,
            value => new WordId(value));

        builder.HasKey(hint => new { hint.WordId, hint.SentenceHash, hint.NormalizedTerm });

        builder.Property(hint => hint.WordId)
            .HasConversion(wordIdConverter)
            .ValueGeneratedNever();

        builder.Property(hint => hint.SentenceHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(hint => hint.NormalizedTerm)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(hint => hint.Term)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(hint => hint.Translation)
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(hint => hint.CreatedAt)
            .IsRequired();

        builder.HasOne<Word>()
            .WithMany()
            .HasForeignKey(hint => hint.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
