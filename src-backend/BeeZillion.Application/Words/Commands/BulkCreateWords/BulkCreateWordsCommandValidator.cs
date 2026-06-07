using FluentValidation;

namespace BeeZillion.Application.Words.Commands.BulkCreateWords;

public sealed class BulkCreateWordsCommandValidator : AbstractValidator<BulkCreateWordsCommand>
{
    public BulkCreateWordsCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Items)
            .Must(items => items.Count <= 300)
            .WithMessage("You can import at most 300 words at once.")
            .When(x => x.Items is not null);

        RuleForEach(x => x.Items)
            .SetValidator(new BulkCreateWordItemValidator());
    }
}

public sealed class BulkCreateWordItemValidator : AbstractValidator<BulkCreateWordItem>
{
    public BulkCreateWordItemValidator()
    {
        RuleFor(x => x.Original)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Translation)
            .NotEmpty()
            .MaximumLength(400);
    }
}