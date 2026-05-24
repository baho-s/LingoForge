namespace BeeZillion.Domain.ValueObjects;

public sealed record Field(string Value)
{
    public Field() : this(string.Empty) { }

    public static Field Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Field name is required.", nameof(value));

        return new Field(value.Trim());
    }
}

