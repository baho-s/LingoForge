using MediatR;
using BeeZillion.Application.Auth.Dtos;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.Aggregates.UserAggregate;
using BeeZillion.Domain.Repositories;

namespace BeeZillion.Application.Auth.Commands.StartGuest;

public sealed class StartGuestCommandHandler : IRequestHandler<StartGuestCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public StartGuestCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(StartGuestCommand request, CancellationToken cancellationToken)
    {
        var user = User.CreateGuest(DateTime.UtcNow);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(_jwtTokenService.GenerateToken(user));
    }
}
