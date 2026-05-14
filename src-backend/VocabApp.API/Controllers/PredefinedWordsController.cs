using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabApp.Application.PredefinedWords.Commands.ImportPredefinedWordsByField;
using VocabApp.Application.PredefinedWords.Queries.GetFieldsList;
using VocabApp.Application.PredefinedWords.Queries.GetPredefinedWordsByField;

namespace VocabApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PredefinedWordsController : ControllerBase
{
    private readonly ISender _sender;

    public PredefinedWordsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get list of available fields (Software, Medicine, Law, etc.)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("fields")]
    public async Task<ActionResult<GetFieldsListResponse>> GetFields(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFieldsListQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get predefined words for a specific field with count
    /// </summary>
    [AllowAnonymous]
    [HttpGet("fields/{field}")]
    public async Task<ActionResult<GetPredefinedWordsByFieldResponse>> GetWordsByField(
        string field,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetPredefinedWordsByFieldQuery(field),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Import all predefined words for a field to user's collection
    /// </summary>
    [Authorize]
    [HttpPost("import-field")]
    public async Task<ActionResult<ImportPredefinedWordsByFieldResponse>> ImportField(
        [FromBody] ImportFieldRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ImportPredefinedWordsByFieldCommand(request.Field),
            cancellationToken);
        return Ok(result);
    }

    public sealed record ImportFieldRequest(string Field);
}
