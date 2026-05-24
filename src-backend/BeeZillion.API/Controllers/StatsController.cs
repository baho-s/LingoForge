using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BeeZillion.Application.Users.Queries.GetStats;

namespace BeeZillion.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class StatsController : ControllerBase
{
    private readonly ISender _sender;

    public StatsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<StatsDto>> Get(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStatsQuery(), cancellationToken);
        return Ok(result);
    }
}

