using MediatR;
using Microsoft.Extensions.Logging;
using BeeZillion.Application.Common.Events;
using BeeZillion.Domain.Enums;
using BeeZillion.Domain.Events;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Users.Events;

public sealed class UserStreakUpdatedEventHandler//Bu sýnýf kullanýcýnýn günlük çalýþma alýþkanlýðýný takip eden bir olay iþleyicisidir.
                                                 //Kullanýcýnýn günlük çalýþma alýþkanlýðý güncellendiðinde, bu sýnýf ilgili olayý dinler
                                                 //ve kullanýcýnýn belirli bir günlük çalýþma alýþkanlýðýna ulaþýp ulaþmadýðýný kontrol eder
                                                 //. Eðer kullanýcý 7 günlük veya 30 günlük bir çalýþma alýþkanlýðýna ulaþmýþsa, ona
                                                 //uygun bir rozet (badge) verir. Bu sayede kullanýcýlarýn motivasyonunu artýrmayý amaçlar.
    : INotificationHandler<DomainEventNotification<UserStreakUpdatedEvent>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserStreakUpdatedEventHandler> _logger;

    public UserStreakUpdatedEventHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserStreakUpdatedEventHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserStreakUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        if (domainEvent.NewStreak is not (7 or 30))
        {
            return;
        }

        var user = await _userRepository.GetByIdAsync(domainEvent.UserId, cancellationToken);
        if (user is null)
        {
            return;
        }

        var badge = domainEvent.NewStreak == 7
            ? BadgeType.SevenDayStreak
            : BadgeType.ThirtyDayStreak;

        user.AwardBadge(badge);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Streak badge awarded: {Badge} for user {UserId}", badge, user.Id.Value);
    }
}

