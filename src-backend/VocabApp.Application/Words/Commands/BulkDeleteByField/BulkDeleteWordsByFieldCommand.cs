using MediatR;

namespace VocabApp.Application.Words.Commands.BulkDeleteByField;

public sealed record BulkDeleteWordsByFieldCommand(string Field) : IRequest<BulkDeleteWordsByFieldResponse>;

public sealed record BulkDeleteWordsByFieldResponse(
    bool Success,
    string FieldName,
    int DeletedCount,
    string Message);
