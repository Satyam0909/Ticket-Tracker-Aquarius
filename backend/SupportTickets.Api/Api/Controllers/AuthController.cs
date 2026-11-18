using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportTickets.Api.Api.Contracts;
using SupportTickets.Api.Api.Constants;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Exceptions;

namespace SupportTickets.Api.Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth-api")]
[Route("api/auth")]
public sealed class AuthController(IAccountService accounts, IWebHostEnvironment environment) : ControllerBase
{
    private const string AccessCookiePath = "/";
    private const string RefreshCookiePath = "/api/auth";

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
        SetSessionCookies(session);
        return Ok(new AccountSessionResponse(session.Email, session.ExpiresAt));
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

        SetSessionCookies(session);
        return Ok(new AccountSessionResponse(session.Email, session.ExpiresAt));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AccountSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountSessionResponse>> Refresh(CancellationToken cancellationToken)
    {
        var token = Request.Cookies[AuthCookieNames.RefreshToken];
        var session = token is null
            ? null
            : await accounts.RefreshAsync(token, cancellationToken);
        if (session is null)
            return Unauthorized(CreateInvalidRefreshProblem());

        SetSessionCookies(session);
        return Ok(new AccountSessionResponse(session.Email, session.ExpiresAt));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var token))
            await accounts.RevokeRefreshTokenAsync(token, cancellationToken);

        Response.Cookies.Delete(AuthCookieNames.AccessToken, CreateCookieOptions(AccessCookiePath));
        Response.Cookies.Delete(AuthCookieNames.RefreshToken, CreateCookieOptions(RefreshCookiePath));
        return NoContent();
    }

    private void SetSessionCookies(AccountSession session)
    {
        Response.Cookies.Append(
            AuthCookieNames.AccessToken,
            session.AccessToken,
            CreateCookieOptions(AccessCookiePath, session.ExpiresAt));
        Response.Cookies.Append(
            AuthCookieNames.RefreshToken,
            session.RefreshToken,
            CreateCookieOptions(RefreshCookiePath, session.RefreshTokenExpiresAt));
    }

    private CookieOptions CreateCookieOptions(string path, DateTime? expiresAt = null)
    {
        var secure = !environment.IsDevelopment() && !environment.IsEnvironment("Testing");
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
            Path = path,
            IsEssential = true,
            Expires = expiresAt
        };
    }

    private ProblemDetails CreateInvalidRefreshProblem()
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Session expired",
            Detail = "Sign in again to continue."
        };
        problem.Extensions["code"] = "invalid_refresh_token";
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return problem;
    }
}
