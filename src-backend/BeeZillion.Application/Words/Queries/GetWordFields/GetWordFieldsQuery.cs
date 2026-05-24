using MediatR;

namespace BeeZillion.Application.Words.Queries.GetWordFields;

public sealed record GetWordFieldsQuery() : IRequest<GetWordFieldsResponse>;

