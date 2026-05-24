using MediatR;

namespace BeeZillion.Application.Words.Commands.BulkGenerate;

public sealed record BulkGenerateSentencesCommand : IRequest<BulkGenerateResult>;

public sealed record BulkGenerateResult(int Generated, int Skipped);

