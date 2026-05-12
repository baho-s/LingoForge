using FluentValidation;

namespace VocabApp.Application.Words.Commands.CreateWord;

public sealed class CreateWordCommandValidator : AbstractValidator<CreateWordCommand>
{
    public CreateWordCommandValidator()
    {
        RuleFor(x => x.Original)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Translation)
            .NotEmpty()
            .MaximumLength(400);
    }
}
