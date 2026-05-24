using FluentValidation;

namespace BeeZillion.Application.Words.Commands.BulkDeleteByField;

public sealed class BulkDeleteWordsByFieldCommandValidator : AbstractValidator<BulkDeleteWordsByFieldCommand>
{
    public BulkDeleteWordsByFieldCommandValidator()
    {
        RuleFor(c => c.Field)
            .Must(f => f == "_no_field" || !string.IsNullOrWhiteSpace(f))
            .WithMessage("Field is required or must be '_no_field'.")
            .MaximumLength(100).WithMessage("Field must not exceed 100 characters.");
    }
}

