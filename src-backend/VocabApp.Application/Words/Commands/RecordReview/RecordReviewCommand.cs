using MediatR;
using VocabApp.Application.Words.Dtos;
using VocabApp.Domain.Enums;

namespace VocabApp.Application.Words.Commands.RecordReview;

public sealed record RecordReviewCommand(Guid WordId, ReviewOutcome Outcome) : IRequest<WordDto>;
