// VocabApp.Infrastructure/AI/GroqService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using VocabApp.Application.Common.Interfaces;

namespace VocabApp.Infrastructure.AI;

public sealed class GroqService : IAiSentenceService
{
    private readonly HttpClient _httpClient;
    private readonly GroqOptions _options;

    public GroqService(HttpClient httpClient, IOptions<GroqOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateSentenceAsync(string word, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Groq ApiKey is not configured.");

        var requestBody = new ChatRequest(
            Model: _options.Model,
            Messages:
            [
                new ChatMessage("system",
                    "You are an English vocabulary assistant. " +
                    "Respond with exactly one sentence. No explanations."),
                new ChatMessage("user",
                    $"Write one natural English sentence that clearly uses the word '{word}'.")
            ],
            MaxTokens: _options.MaxTokens
        );

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/openai/v1/chat/completions")
        {
            Content = JsonContent.Create(requestBody)
        };
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Groq error: {(int)response.StatusCode} {err}");
        }

        var payload = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);

        var sentence = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

        if (string.IsNullOrWhiteSpace(sentence))
            throw new InvalidOperationException("Groq response was empty.");

        // Bazen tırnak veya numara ekler — temizle
        sentence = sentence
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)[0]
            .Trim('"', '\'', ' ');

        return sentence;
    }

    public async Task<AiEvaluationResult> EvaluateTranslationAsync(
        string englishSentence,
        string userTranslation,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Groq ApiKey is not configured.");

        var requestBody = new ChatRequest(
    Model: _options.Model,
    Messages:
    [
        new ChatMessage("system",
            "You are a strict bilingual evaluator. " +
            "Compare the English sentence with the user's Turkish translation. " +
            "Return ONLY valid JSON with these exact fields: " +
            "score (0-100 integer) and feedback (string). " +
            "Focus on semantic accuracy and naturalness. " +
            "At the very end of the feedback, ALWAYS add the correct natural translation in this format: " +
            "[Doğru Çeviri: tam doğru cümle]"),
        
        new ChatMessage("user",
            $"English: {englishSentence}\nTurkish: {userTranslation}\nJSON only.")
    ],
    MaxTokens: Math.Max(80, _options.MaxTokens)
);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/openai/v1/chat/completions")
        {
            Content = JsonContent.Create(requestBody)
        };
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Groq error: {(int)response.StatusCode} {err}");
        }

        var payload = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);

        var content = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Groq response was empty.");

        var json = ExtractJson(content);
        var result = JsonSerializer.Deserialize<EvaluationResult>(json);
        if (result is null)
        {
            throw new InvalidOperationException("Groq response could not be parsed.");
        }

        var score = Math.Clamp(result.Score, 0, 100);
        var feedback = string.IsNullOrWhiteSpace(result.Feedback)
            ? "Degerlendirme tamamlandi."
            : result.Feedback.Trim();

        return new AiEvaluationResult(score, feedback);
    }

    private static string ExtractJson(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return content.Substring(start, end - start + 1);
        }

        return content;
    }

    // ── Request modelleri ────────────────────────────────────────────
    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
        [property: JsonPropertyName("max_tokens")] int MaxTokens
    );

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content
    );

    // ── Response modelleri ───────────────────────────────────────────
    private sealed record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] List<Choice>? Choices
    );

    private sealed record Choice(
        [property: JsonPropertyName("message")] ChatMessage? Message
    );

    private sealed record EvaluationResult(
        [property: JsonPropertyName("score")] int Score,
        [property: JsonPropertyName("feedback")] string Feedback
    );
}