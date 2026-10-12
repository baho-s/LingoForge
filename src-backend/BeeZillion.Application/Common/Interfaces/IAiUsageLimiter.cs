namespace BeeZillion.Application.Common.Interfaces;

public interface IAiUsageLimiter
{
    /// <summary>Misafir kullanıcının AI hakkından düşer; yetersizse AiLimitExceededException fırlatır.</summary>
    Task ConsumeAsync(int count, CancellationToken ct = default);

    /// <summary>Düşmeden yalnızca yeterli hak olduğunu doğrular (toplu işlemlerde ön kontrol).</summary>
    Task EnsureAvailableAsync(int count, CancellationToken ct = default);
}
