namespace VocabApp.Domain.ValueObjects;

public sealed record PredefinedWordId(Guid Value)
{
    public PredefinedWordId() : this(Guid.Empty) { }

    public static PredefinedWordId Create() => new(Guid.NewGuid());
}
