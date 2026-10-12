// BeeZillion.Infrastructure/AI/GroqOptions.cs
namespace BeeZillion.Infrastructure.AI;

public sealed class GroqOptions
{
    public string BaseUrl { get; set; } = "https://api.groq.com";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "openai/gpt-oss-20b";
    public int MaxTokens { get; set; } = 80;
}
