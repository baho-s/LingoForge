using BeeZillion.Application.Common.Interfaces;

namespace BeeZillion.Application.Common.Services;

public sealed class LimitedAiSentenceService : IAiSentenceService
{
    private readonly IAiSentenceService _inner;
    private readonly IAiUsageLimiter _limiter;

    public LimitedAiSentenceService(IAiSentenceService inner, IAiUsageLimiter limiter)
    {
        _inner = inner;
        _limiter = limiter;
    }

    public async Task<string> GenerateSentenceAsync(string word, CancellationToken ct = default)
    {
        await _limiter.ConsumeAsync(1, ct);
        return await _inner.GenerateSentenceAsync(word, ct);
    }

    public async Task<string> TranslateWordInSentenceAsync(
        string englishSentence,
        string word,
        CancellationToken ct = default)
    {
        await _limiter.ConsumeAsync(1, ct);
        return await _inner.TranslateWordInSentenceAsync(englishSentence, word, ct);
    }

    public async Task<AiEvaluationResult> EvaluateTranslationAsync(
        string englishSentence,
        string userTranslation,
        CancellationToken ct = default)
    {
        await _limiter.ConsumeAsync(1, ct);
        return await _inner.EvaluateTranslationAsync(englishSentence, userTranslation, ct);
    }
}
