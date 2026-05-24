using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BeeZillion.Domain.Aggregates.PredefinedWordAggregate;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Infrastructure.Persistence.Configurations;

public sealed class PredefinedWordConfiguration : IEntityTypeConfiguration<PredefinedWord>
{
    public void Configure(EntityTypeBuilder<PredefinedWord> builder)
    {
        var predefinedWordIdConverter = new ValueConverter<PredefinedWordId, Guid>(
            id => id.Value,
            value => new PredefinedWordId(value));

        builder.HasKey(pw => pw.Id);
        builder.Property(pw => pw.Id)
            .HasConversion(predefinedWordIdConverter)
            .ValueGeneratedNever();

        builder.Property(pw => pw.Field)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pw => pw.Category)
            .HasMaxLength(100);

        builder.Property(pw => pw.Level)
            .HasMaxLength(2);

        builder.Property(pw => pw.Original)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pw => pw.Translation)
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(pw => pw.AiSentence)
            .HasMaxLength(1000);

        builder.Property(pw => pw.CreatedAt)
            .IsRequired();

        builder.Property(pw => pw.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(pw => new { pw.Field, pw.IsActive });
        builder.HasIndex(pw => new { pw.Field, pw.Category });
        builder.HasIndex(pw => pw.Original);

        builder.HasData(GetSeedData());
    }

    private static IEnumerable<PredefinedWord> GetSeedData()
    {
        // Software field
        yield return PredefinedWord.Create("Software", "General", "algorithm", "algoritma", "An algorithm is a step-by-step procedure for solving a problem.");
        yield return PredefinedWord.Create("Software", "General", "bug", "hata", "A bug is an error or flaw in a software program.");
        yield return PredefinedWord.Create("Software", "General", "debug", "hata ayıklamak", "To debug means to find and fix errors in code.");
        yield return PredefinedWord.Create("Software", "General", "framework", "framework", "A framework provides a foundation for building applications.");
        yield return PredefinedWord.Create("Software", "General", "repository", "depo", "A repository is a central storage location for code.");

        // Medicine field
        yield return PredefinedWord.Create("Medicine", "General", "diagnosis", "tanı", "Diagnosis is the identification of a disease or condition.");
        yield return PredefinedWord.Create("Medicine", "General", "prognosis", "hastalık gidişi", "Prognosis is the likely outcome of a disease.");
        yield return PredefinedWord.Create("Medicine", "General", "symptom", "semptom", "A symptom is a sign of illness or disease.");
        yield return PredefinedWord.Create("Medicine", "General", "treatment", "tedavi", "Treatment is the medical care given for an illness.");

        // Law field
        yield return PredefinedWord.Create("Law", "General", "defendant", "davalı", "A defendant is a person accused of a crime.");
        yield return PredefinedWord.Create("Law", "General", "verdict", "karar", "A verdict is the decision made by a court.");
        yield return PredefinedWord.Create("Law", "General", "lawsuit", "dava", "A lawsuit is a legal action brought in court.");
        yield return PredefinedWord.Create("Law", "General", "attorney", "avukat", "An attorney is a lawyer who represents clients.");
    }
}

