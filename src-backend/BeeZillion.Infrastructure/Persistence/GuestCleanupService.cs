using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BeeZillion.Infrastructure.Persistence;

public sealed class GuestCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GuestCleanupService> _logger;

    public GuestCleanupService(IServiceScopeFactory scopeFactory, ILogger<GuestCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await DeleteExpiredGuestsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Guest cleanup failed.");
            }
        }
        while (await WaitNextAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitNextAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task DeleteExpiredGuestsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        var expiredIds = await db.Users
            .Where(user => user.IsGuest && user.GuestExpiresAt <= now)
            .Select(user => user.Id)
            .ToListAsync(ct);

        if (expiredIds.Count == 0)
        {
            return;
        }

        // SentenceWordHints, Word silindiğinde cascade ile temizlenir.
        await db.Words.Where(word => expiredIds.Contains(word.OwnerId)).ExecuteDeleteAsync(ct);
        await db.ReviewHistories.Where(history => expiredIds.Contains(history.UserId)).ExecuteDeleteAsync(ct);
        await db.UserVocabularyProgresses.Where(progress => expiredIds.Contains(progress.UserId)).ExecuteDeleteAsync(ct);
        var deleted = await db.Users.Where(user => expiredIds.Contains(user.Id)).ExecuteDeleteAsync(ct);

        _logger.LogInformation("Deleted {Count} expired guest accounts.", deleted);
    }
}
