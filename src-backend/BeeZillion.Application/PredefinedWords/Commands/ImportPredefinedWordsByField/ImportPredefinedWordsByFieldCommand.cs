using MediatR;

namespace BeeZillion.Application.PredefinedWords.Commands.ImportPredefinedWordsByField;

public sealed record ImportPredefinedWordsByFieldCommand(string Field) : IRequest<ImportPredefinedWordsByFieldResponse>;

public sealed record ImportPredefinedWordsByFieldResponse(
    bool Success,
    string FieldName,
    int ImportedCount,
    string Message);

