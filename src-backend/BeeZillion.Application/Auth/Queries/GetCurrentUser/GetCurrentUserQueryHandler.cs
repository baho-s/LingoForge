using MediatR;
using BeeZillion.Application.Auth.Dtos;
using BeeZillion.Application.Common.Exceptions;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Aggregates.UserAggregate;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(IUserRepository userRepository, ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.GetUserId(), cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return user.IsGuest
            ? new CurrentUserResponse(true, user.GuestExpiresAt, User.GuestAiLimit, user.GetGuestAiRemaining())
            : new CurrentUserResponse(false, null, null, null);
    }
}
