// VocabApp.Infrastructure/AI/GroqOptions.cs
namespace VocabApp.Infrastructure.AI;

public sealed class GroqOptions
{
    public string BaseUrl { get; init; } = "https://api.groq.com";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "llama-3.1-8b-instant"; // ücretsiz, hýzlý
    public int MaxTokens { get; init; } = 80;
}