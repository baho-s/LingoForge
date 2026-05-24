using MediatR;

namespace BeeZillion.Application.PredefinedWords.Queries.GetFieldsList;

public sealed record GetFieldsListQuery : IRequest<GetFieldsListResponse>;

public sealed record GetFieldsListResponse(IReadOnlyList<string> Fields);

