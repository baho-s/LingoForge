namespace BeeZillion.Application.Auth.Dtos;

public sealed record AuthResponse(string Token);

public sealed record CurrentUserResponse(bool IsGuest, DateTime? GuestExpiresAt, int? AiLimit, int? AiRemaining);

