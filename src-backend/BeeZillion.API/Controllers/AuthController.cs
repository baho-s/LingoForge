using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BeeZillion.Application.Auth.Commands.Login;
using BeeZillion.Application.Auth.Commands.Register;
using BeeZillion.Application.Auth.Commands.StartGuest;
using BeeZillion.Application.Auth.Dtos;
using BeeZillion.Application.Auth.Queries.GetCurrentUser;

namespace BeeZillion.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [EnableRateLimiting("guest")]
    [HttpPost("guest")]
    public async Task<ActionResult<AuthResponse>> StartGuest(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartGuestCommand(), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

