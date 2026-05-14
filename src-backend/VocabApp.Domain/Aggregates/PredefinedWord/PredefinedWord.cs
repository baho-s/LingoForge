using VocabApp.Domain.Common;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Domain.Aggregates.PredefinedWordAggregate;

public sealed class PredefinedWord : AggregateRoot<PredefinedWordId>
{
    public string Field { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public string Original { get; private set; } = string.Empty;
    public string Translation { get; private set; } = string.Empty;
    public string? AiSentence { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private PredefinedWord() { }

    private PredefinedWord(
        PredefinedWordId id,
        string field,
        string? category,
        string original,
        string translation,
        string? aiSentence)
        : base(id)
    {
        Field = field;
        Category = category;
        Original = original;
        Translation = translation;
        AiSentence = aiSentence;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static PredefinedWord Create(
        string field,
        string? category,
        string original,
        string translation,
        string? aiSentence = null)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field is required.", nameof(field));

        if (string.IsNullOrWhiteSpace(original))
            throw new ArgumentException("Original word is required.", nameof(original));

        if (string.IsNullOrWhiteSpace(translation))
            throw new ArgumentException("Translation is required.", nameof(translation));

        return new PredefinedWord(
            PredefinedWordId.Create(),
            field.Trim(),
            category?.Trim(),
            original.Trim(),
            translation.Trim(),
            aiSentence?.Trim());
    }

    public void AttachAiSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            throw new ArgumentException("Sentence is required.", nameof(sentence));

        AiSentence = sentence.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
