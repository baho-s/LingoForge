using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Domain.Entities;

public sealed class SentenceWordHint
{
    public WordId WordId { get; private set; } = default!;
    public string SentenceHash { get; private set; } = string.Empty;
    public string NormalizedTerm { get; private set; } = string.Empty;
    public string Term { get; private set; } = string.Empty;
    public string Translation { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private SentenceWordHint() { }

    private SentenceWordHint(WordId wordId, string sentenceHash, string term, string translation)
    {
        WordId = wordId;
        SentenceHash = sentenceHash;
        Term = term.Trim();
        NormalizedTerm = NormalizeTerm(term);
        Translation = translation.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public static SentenceWordHint Create(
        WordId wordId,
        string sentenceHash,
        string term,
        string translation)
    {
        if (string.IsNullOrWhiteSpace(sentenceHash))
        {
            throw new ArgumentException("Sentence hash is required.", nameof(sentenceHash));
        }

        if (string.IsNullOrWhiteSpace(term))
        {
            throw new ArgumentException("Term is required.", nameof(term));
        }

        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new ArgumentException("Translation is required.", nameof(translation));
        }

        if (term.Trim().Length > 200)
        {
            throw new ArgumentException("Term cannot exceed 200 characters.", nameof(term));
        }

        if (translation.Trim().Length > 400)
        {
            throw new ArgumentException("Translation cannot exceed 400 characters.", nameof(translation));
        }

        return new SentenceWordHint(wordId, sentenceHash, term, translation);
    }

    public static string NormalizeTerm(string term)
    {
        return string.Join(
            " ",
            term.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ).ToLowerInvariant();
    }
}
