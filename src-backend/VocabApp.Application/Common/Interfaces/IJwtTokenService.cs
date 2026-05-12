using VocabApp.Domain.Aggregates.UserAggregate;

namespace VocabApp.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
