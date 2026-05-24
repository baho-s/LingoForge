using BeeZillion.Domain.ValueObjects;

namespace BeeZillion.Application.Common.Interfaces;

public interface ICurrentUserService
{
    UserId GetUserId();
}

