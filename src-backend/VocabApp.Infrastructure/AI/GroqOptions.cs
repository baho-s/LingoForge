// VocabApp.Infrastructure/AI/GroqOptions.cs
namespace VocabApp.Infrastructure.AI;

public sealed class GroqOptions
{
    public string BaseUrl { get; set; } = "https://api.groq.com";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.1-8b-instant";
    public int MaxTokens { get; set; } = 80;
}