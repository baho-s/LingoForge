using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabApp.Application.Words.Queries.GetQuizWords;

namespace VocabApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class QuizController : ControllerBase
{
    private readonly ISender _sender;

    public QuizController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuizWordDto>>> Get(
        [FromQuery] QuizMode mode = QuizMode.FillBlank,
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetQuizWordsQuery(mode, count), cancellationToken);
        return Ok(result);
    }
}
