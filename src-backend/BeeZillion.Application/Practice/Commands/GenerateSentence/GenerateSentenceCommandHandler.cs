using MediatR;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Practice.Dtos;

namespace BeeZillion.Application.Practice.Commands.GenerateSentence;

public sealed class GenerateSentenceCommandHandler : IRequestHandler<GenerateSentenceCommand, GeneratedSentenceResponse>
{
    private readonly IAiSentenceService _aiSentenceService;

    public GenerateSentenceCommandHandler(IAiSentenceService aiSentenceService)
    {
        _aiSentenceService = aiSentenceService;
    }

    public async Task<GeneratedSentenceResponse> Handle(
        GenerateSentenceCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TargetVocab == null || request.TargetVocab.Count == 0)
        {
            throw new ArgumentException("target_vocab is required.");
        }

        var prompt = string.Join(", ", request.TargetVocab);
        var sentence = await _aiSentenceService.GenerateSentenceAsync(prompt, cancellationToken);

        return new GeneratedSentenceResponse(
            Guid.NewGuid().ToString(),
            sentence,
            request.TargetVocab);
    }
}

