namespace BeeZillion.Application.Common.Exceptions;

public sealed class AiLimitExceededException : Exception
{
    public AiLimitExceededException(string message) : base(message) { }
}
