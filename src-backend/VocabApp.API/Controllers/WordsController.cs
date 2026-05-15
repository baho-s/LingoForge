using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabApp.Application.Words.Commands.BulkGenerate;
using VocabApp.Application.Words.Commands.BulkDeleteByField;
using VocabApp.Application.Words.Commands.CreateWord;
using VocabApp.Application.Words.Commands.DeleteWord;
using VocabApp.Application.Words.Commands.RecordReview;
using VocabApp.Application.Words.Dtos;
using VocabApp.Application.Words.Queries.GetWordList;
using VocabApp.Application.Words.Queries.GetWordOfDay;
using VocabApp.Application.Words.Queries.GetReviewSessionWords;

namespace VocabApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class WordsController : ControllerBase
{
    private readonly ISender _sender;

    public WordsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WordDto>>> GetList(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetWordListQuery(skip, take), cancellationToken);
        return Ok(result);
    }

    [HttpGet("review/session")]
    public async Task<ActionResult<IReadOnlyList<WordDto>>> GetReviewSessionWords(
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetReviewSessionWordsQuery(limit), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WordDto>> Create(CreateWordCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("bulk-generate")]
    public async Task<ActionResult<BulkGenerateResult>> BulkGenerate(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new BulkGenerateSentencesCommand(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Bulk delete all words from a specific field
    /// </summary>
    [HttpDelete("by-field/{field}")]
    public async Task<ActionResult<BulkDeleteWordsByFieldResponse>> BulkDeleteByField(
        string field,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new BulkDeleteWordsByFieldCommand(field),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<WordDto>> RecordReview(
        Guid id,
        [FromBody] RecordReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RecordReviewCommand(id, request.Outcome);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("word-of-day")]
    public async Task<ActionResult<WordDto>> GetWordOfDay(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWordOfDayQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteWordCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record RecordReviewRequest(VocabApp.Domain.Enums.ReviewOutcome Outcome);
}
