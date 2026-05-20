// VocabApp.Infrastructure/AI/GroqService.cs
using System.Collections.Generic;
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
                    "You are a strict bilingual evaluator for an English-to-Turkish translation test. " +
                    "Rules: The user's answer must be Turkish. If the user repeats the English sentence or answers in English, score must be 0-20 and error_summary must say it is in English. " +
                    "Always return valid JSON with exactly these fields: " +
                    "{\"score\": <0-100>, \"error_summary\": \"<short Turkish error or praise>\", \"correct_translation\": \"<natural Turkish translation>\"}. " +
                    "Score by semantic accuracy. Keep error_summary short and specific (mention the key mistake if any)."),
                new ChatMessage("user",
                    $"English sentence: {englishSentence}\n\nUser's Turkish translation: {userTranslation}\n\nRespond with valid JSON only.")
            ],
            MaxTokens: Math.Max(120, _options.MaxTokens)
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
                return new AiEvaluationResult(defaultScore, "Değerlendirme tamamlandı.", null);
            }

            var score = Math.Clamp(result.Score, 0, 100);
            var feedback = string.IsNullOrWhiteSpace(result.ErrorSummary)
                ? "Değerlendirme tamamlandı."
                : result.ErrorSummary.Trim();
            var correctTranslation = string.IsNullOrWhiteSpace(result.CorrectTranslation)
                ? null
                : result.CorrectTranslation.Trim();

            if (IsLikelyEnglishAnswer(userTranslation) || IsEnglishCopy(englishSentence, userTranslation))
            {
                var forcedScore = Math.Min(score, 20);
                var forcedFeedback = "Cevap Türkçe olmalı; İngilizce metni aynen yazmışsın.";
                return new AiEvaluationResult(forcedScore, forcedFeedback, correctTranslation);
            }

            return new AiEvaluationResult(score, feedback, correctTranslation);
        }
        catch (JsonException ex)
        {
            // JSON parsing başarısız olursa fallback response
            System.Diagnostics.Debug.WriteLine($"JSON parsing failed: {ex.Message}. Content: {content}");
            var defaultScore = userTranslation.Length > englishSentence.Length / 2 ? 60 : 30;
            return new AiEvaluationResult(defaultScore, "Değerlendirme tamamlandı.", null);
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

    private static bool IsLikelyEnglishAnswer(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (ContainsTurkishChars(text))
        {
            return false;
        }

        if (ContainsTurkishCommonWord(text))
        {
            return false;
        }

        var letterCount = 0;
        var englishLetterCount = 0;

        foreach (var ch in text)
        {
            if (char.IsLetter(ch))
            {
                letterCount += 1;
                if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                {
                    englishLetterCount += 1;
                }
            }
        }

        if (letterCount == 0)
        {
            return false;
        }

        return englishLetterCount / (double)letterCount >= 0.85;
    }

    private static bool ContainsTurkishCommonWord(string text)
    {
        var normalized = NormalizeForCompare(text);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return false;
        }

        var common = new HashSet<string>(StringComparer.Ordinal)
        {
            "ve", "bir", "bu", "su", "icin", "degil", "ama", "gibi",
            "olan", "daha", "cok", "ben", "sen", "o", "biz", "siz",
            "onlar", "ile", "mi", "mu", "mu", "midir", "neden",
            "ne", "kim", "nerede", "nasil", "kadar"
        };

        foreach (var token in tokens)
        {
            if (common.Contains(token))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEnglishCopy(string englishSentence, string userTranslation)
    {
        var normEnglish = NormalizeForCompare(englishSentence);
        var normUser = NormalizeForCompare(userTranslation);

        if (string.IsNullOrWhiteSpace(normEnglish) || string.IsNullOrWhiteSpace(normUser))
        {
            return false;
        }

        if (string.Equals(normEnglish, normUser, StringComparison.Ordinal))
        {
            return true;
        }

        var englishTokens = normEnglish.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var userTokens = normUser.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (englishTokens.Length == 0 || userTokens.Length == 0)
        {
            return false;
        }

        var intersection = 0;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in englishTokens)
        {
            seen.Add(token);
        }
        foreach (var token in userTokens)
        {
            if (seen.Contains(token))
            {
                intersection += 1;
            }
        }

        var union = englishTokens.Length + userTokens.Length - intersection;
        if (union <= 0)
        {
            return false;
        }

        var similarity = intersection / (double)union;
        return similarity >= 0.7;
    }

    private static string NormalizeForCompare(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var buffer = new char[text.Length];
        var idx = 0;
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                buffer[idx++] = char.ToLowerInvariant(ch);
            }
        }

        return new string(buffer, 0, idx).Trim();
    }

    private static bool ContainsTurkishChars(string text)
    {
        foreach (var ch in text)
        {
            switch (ch)
            {
                case 'ç':
                case 'ğ':
                case 'ı':
                case 'ö':
                case 'ş':
                case 'ü':
                case 'Ç':
                case 'Ğ':
                case 'İ':
                case 'Ö':
                case 'Ş':
                case 'Ü':
                    return true;
            }
        }

        return false;
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
        [property: JsonPropertyName("error_summary")] string ErrorSummary,
        [property: JsonPropertyName("correct_translation")] string CorrectTranslation
    );
}