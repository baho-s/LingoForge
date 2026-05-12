using FluentValidation;

namespace VocabApp.Application.Words.Commands.RecordReview;

public sealed class RecordReviewCommandValidator : AbstractValidator<RecordReviewCommand>
{
    public RecordReviewCommandValidator()
    {
        RuleFor(x => x.WordId).NotEmpty();
    }
}
