using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Aggregates.UserAggregate;
using BeeZillion.Domain.Repositories;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Application.Common.Services;

public sealed class AiUsageLimiter : IAiUsageLimiter
{
    private const string LimitMessage = "Guest AI limit reached. Please register to continue.";

    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepository;

    public AiUsageLimiter(ICurrentUserService currentUser, IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task ConsumeAsync(int count, CancellationToken ct = default)
    {
        var userId = _currentUser.GetUserId();
        var guest = await GetActiveGuestAsync(userId, ct);
        if (guest is null)
        {
            return;
        }

        if (!await _userRepository.TryConsumeGuestAiCallsAsync(userId, count, User.GuestAiLimit, ct))
        {
            throw new AiLimitExceededException(LimitMessage);
        }
    }

    public async Task EnsureAvailableAsync(int count, CancellationToken ct = default)
    {
        var guest = await GetActiveGuestAsync(_currentUser.GetUserId(), ct);
        if (guest is not null && guest.GetGuestAiRemaining() < count)
        {
            throw new AiLimitExceededException(LimitMessage);
        }
    }

    private async Task<User?> GetActiveGuestAsync(UserId userId, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null || !user.IsGuest)
        {
            return null;
        }

        if (user.IsGuestExpired(DateTime.UtcNow))
        {
            throw new ForbiddenException("Guest session expired.");
        }

        return user;
    }
}
