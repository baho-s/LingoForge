using MediatR;
using BeeZillion.Application.Words.Dtos;
using BeeZillion.Domain.Enums;

namespace BeeZillion.Application.Words.Commands.RecordReview;

public sealed record RecordReviewCommand(Guid WordId, ReviewOutcome Outcome) : IRequest<WordDto>;

