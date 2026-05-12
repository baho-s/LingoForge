using MediatR;
using VocabApp.Application.Auth.Dtos;
using VocabApp.Application.Common.Exceptions;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;

namespace VocabApp.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !_passwordHasher.VerifyHashedPassword(user.PasswordHash, request.Password))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Credentials"] = new[] { "Invalid email or password." },
            });
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponse(token);
    }
}
