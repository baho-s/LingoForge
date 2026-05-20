using MediatR;

namespace VocabApp.Application.Words.Queries.GetWordFields;

public sealed record GetWordFieldsQuery() : IRequest<GetWordFieldsResponse>;
