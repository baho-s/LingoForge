using BeeZillion.Domain.Enums;

namespace BeeZillion.Domain.Entities;

public sealed record Badge(BadgeType Type, DateTime AwardedAt);

