using FluentValidation;

namespace VocabApp.Application.Words.Queries.GetQuizWords;

public sealed class GetQuizWordsQueryValidator : AbstractValidator<GetQuizWordsQuery>
{
    public GetQuizWordsQueryValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .LessThanOrEqualTo(20);
    }
}
