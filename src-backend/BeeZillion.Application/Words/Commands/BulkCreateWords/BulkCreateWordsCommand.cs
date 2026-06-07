using MediatR;

namespace BeeZillion.Application.Words.Commands.BulkCreateWords;

public sealed record BulkCreateWordItem(string Original, string Translation);

public sealed record BulkCreateWordsCommand(
    IReadOnlyList<BulkCreateWordItem> Items,
    bool GenerateSentenceImmediately) : IRequest<BulkCreateWordsResult>;

public sealed record BulkCreateWordsResult(
    int CreatedCount,
    int GeneratedSentenceCount);