using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        // .NET varsayýlan olarak "sub" claim'ini NameIdentifier'a dönüþtürür.
        // Bu yüzden öncelikle ClaimTypes.NameIdentifier'a bakýyoruz.
        var value = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (value is null || !Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("User id is missing from token.");
        }

        return new UserId(userId);
    }
}
