using FluentValidation;

namespace BeeZillion.Application.PredefinedWords.Commands.ImportPredefinedWordsByField;

public sealed class ImportPredefinedWordsByFieldCommandValidator : AbstractValidator<ImportPredefinedWordsByFieldCommand>
{
    public ImportPredefinedWordsByFieldCommandValidator()
    {
        RuleFor(c => c.Field)
            .NotEmpty().WithMessage("Field is required.")
            .MaximumLength(100).WithMessage("Field must not exceed 100 characters.");
    }
}

