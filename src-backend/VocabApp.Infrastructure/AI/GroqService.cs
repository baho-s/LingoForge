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
                    "Generate exactly one natural English sentence using the given word. " +
                    "IMPORTANT: Keep vocabulary at B2 level (Upper Intermediate) - use common, intermediate-level English. " +
                    "Avoid complex academic or C1/C2 words. No explanations, just the sentence."),
                new ChatMessage("user",
                    $"Write one natural B2-level English sentence that uses the word '{word}'.")
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
            "Return a JSON response with exactly these fields: " +
            "{\"score\": <0-100>, \"feedback\": \"<evaluation text>\"}. " +
            "Score the translation on semantic accuracy (0-100). " +
            "Keep feedback concise and in Turkish."),
        
        new ChatMessage("user",
            $"English sentence: {englishSentence}\n\nUser's Turkish translation: {userTranslation}\n\nRespond with valid JSON only.")
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

        try
        {
            var json = ExtractJson(content);
            var result = JsonSerializer.Deserialize<EvaluationResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (result is null)
            {
                // Fallback: benimse cevabı basit heuristic'le
                var defaultScore = userTranslation.Length > englishSentence.Length / 2 ? 60 : 30;
                return new AiEvaluationResult(defaultScore, "Değerlendirme tamamlandı.");
            }

            var score = Math.Clamp(result.Score, 0, 100);
            var feedback = string.IsNullOrWhiteSpace(result.Feedback)
                ? "Değerlendirme tamamlandı."
                : result.Feedback.Trim();

            return new AiEvaluationResult(score, feedback);
        }
        catch (JsonException ex)
        {
            // JSON parsing başarısız olursa fallback response
            System.Diagnostics.Debug.WriteLine($"JSON parsing failed: {ex.Message}. Content: {content}");
            var defaultScore = userTranslation.Length > englishSentence.Length / 2 ? 60 : 30;
            return new AiEvaluationResult(defaultScore, "Değerlendirme tamamlandı.");
        }
    }

    private static string ExtractJson(string content)
    {
        // Tırnak içine alınmış JSON'ı kontrol et
        if (content.StartsWith("\"") && content.EndsWith("\""))
        {
            content = content.Substring(1, content.Length - 2);
        }

        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        
        if (start >= 0 && end > start)
        {
            return content.Substring(start, end - start + 1);
        }

        // Eğer JSON bulunamazsa, content'in kendisini return et (muhtemelen zaten JSON'dır)
        return content.Trim();
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