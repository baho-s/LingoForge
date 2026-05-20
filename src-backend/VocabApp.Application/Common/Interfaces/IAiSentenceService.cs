namespace VocabApp.Application.Common.Interfaces;

public interface IAiSentenceService
{
    Task<string> GenerateSentenceAsync(string word, CancellationToken ct = default);
    Task<AiEvaluationResult> EvaluateTranslationAsync(
        string englishSentence,
        string userTranslation,
        CancellationToken ct = default);
}

public sealed record AiEvaluationResult(int Score, string Feedback, string? CorrectTranslation);
