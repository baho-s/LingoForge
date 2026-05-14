using FluentValidation;

namespace VocabApp.Application.Words.Commands.BulkDeleteByField;

public sealed class BulkDeleteWordsByFieldCommandValidator : AbstractValidator<BulkDeleteWordsByFieldCommand>
{
    public BulkDeleteWordsByFieldCommandValidator()
    {
        RuleFor(c => c.Field)
            .NotEmpty().WithMessage("Field is required.")
            .MaximumLength(100).WithMessage("Field must not exceed 100 characters.");
    }
}
