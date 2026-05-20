using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using VocabApp.Application.Practice.Commands.GenerateSentence;
using VocabApp.Application.Practice.Commands.SubmitPracticeAnswer;
using VocabApp.Application.Practice.Dtos;
using VocabApp.Application.Practice.Queries.GetPracticeQuestions;

namespace VocabApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PracticeController : ControllerBase
{
    private readonly ISender _sender;

    public PracticeController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("questions")]
    public async Task<ActionResult<PracticeQuestionsResponse>> GetQuestions(
        [FromQuery] string? mode = null,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetPracticeQuestionsQuery(mode, limit),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("generate-sentence")]
    public async Task<ActionResult<GeneratedSentenceResponse>> GenerateSentence(
        [FromBody] GenerateSentenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GenerateSentenceCommand(request.TargetVocab.ToList()),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("submit-answer")]
    public async Task<ActionResult<PracticeAnswerResponse>> SubmitAnswer(
        [FromBody] PracticeAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new SubmitPracticeAnswerCommand(
                request.QuestionId,
                request.UserAnswer,
                request.Type,
                request.Direction,
                request.TimeTakenMs),
            cancellationToken);
        return Ok(result);
    }

    public sealed record GenerateSentenceRequest(
        [property: JsonPropertyName("target_vocab")] IReadOnlyList<string> TargetVocab);

    public sealed record PracticeAnswerRequest(
        [property: JsonPropertyName("question_id")] string QuestionId,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("user_answer")] string UserAnswer,
        [property: JsonPropertyName("direction")] string? Direction,
        [property: JsonPropertyName("time_taken_ms")] long TimeTakenMs = 0);
}
