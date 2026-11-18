using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportTickets.Api.Api.Contracts;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Exceptions;

namespace SupportTickets.Api.Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth-api")]
[Route("api/auth")]
public sealed class AuthController(IAccountService accounts) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AccountSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountSessionResponse>> Register(
        RegisterAccountRequest request,
        CancellationToken cancellationToken)
    {
        var session = await accounts.RegisterAsync(
            new RegisterAccountCommand(request.Email, request.Password),
            cancellationToken);
        return Ok(new AccountSessionResponse(session.Email, session.AccessToken, session.ExpiresAt));
    }

    [HttpPost("login")]
    [ProducesResponseType<AccountSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountSessionResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var session = await accounts.LoginAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);
        if (session is null)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Sign-in failed",
                Detail = "The email or password is incorrect."
            };
            problem.Extensions["code"] = "invalid_credentials";
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            return Unauthorized(problem);
        }

        return Ok(new AccountSessionResponse(session.Email, session.AccessToken, session.ExpiresAt));
    }
}
