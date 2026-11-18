using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Api.Api.Constants;
using SupportTickets.Api.Application.Exceptions;

namespace SupportTickets.Api.Api.Middleware;

public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request {TraceId} was cancelled by the client.", context.TraceIdentifier);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (statusCode, code, title, detail) = exception switch
            {
                TicketNotFoundException =>
                    (StatusCodes.Status404NotFound, ApiErrorCodes.TicketNotFound,
                        "Not found", ApiMessages.TicketNotFound),
                AccountAlreadyExistsException =>
                    (StatusCodes.Status409Conflict, ApiErrorCodes.AccountAlreadyExists,
                        "Account already exists", ApiMessages.AccountAlreadyExists),
                _ =>
                    (StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalError,
                        "Server error", ApiMessages.InternalError)
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception for request {TraceId}.", context.TraceIdentifier);
            else
                logger.LogInformation("Request {TraceId} failed with {ErrorCode}.", context.TraceIdentifier, code);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = context.Request.Path
            };
            problem.Extensions["code"] = code;
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
        }
    }
}
