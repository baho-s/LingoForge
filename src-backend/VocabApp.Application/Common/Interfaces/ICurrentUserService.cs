using VocabApp.Domain.ValueObjects;

namespace VocabApp.Application.Common.Interfaces;

public interface ICurrentUserService
{
    UserId GetUserId();
}
