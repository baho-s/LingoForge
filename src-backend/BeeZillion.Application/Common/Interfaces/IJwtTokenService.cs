using BeeZillion.Domain.Aggregates.UserAggregate;

namespace BeeZillion.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}

