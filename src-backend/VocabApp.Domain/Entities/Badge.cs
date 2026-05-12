using VocabApp.Domain.Enums;

namespace VocabApp.Domain.Entities;

public sealed record Badge(BadgeType Type, DateTime AwardedAt);
