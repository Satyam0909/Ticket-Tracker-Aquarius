using Microsoft.AspNetCore.Mvc;

namespace SupportTickets.Api.Api.Middleware;

public sealed class TrustedOriginMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly HashSet<string> allowedOrigins = configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()?
        .Select(NormalizeOrigin)
        .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsOptions(context.Request.Method) ||
            HttpMethods.IsGet(context.Request.Method) ||
            HttpMethods.IsHead(context.Request.Method) ||
            HttpMethods.IsTrace(context.Request.Method) ||
            !context.Request.Headers.TryGetValue("Origin", out var origin))
        {
            await next(context);
            return;
        }

        if (!allowedOrigins.Contains(NormalizeOrigin(origin.ToString())))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Request origin is not allowed",
                Detail = "This request origin is not trusted."
            });
            return;
        }

        await next(context);
    }

    private static string NormalizeOrigin(string origin) => origin.Trim().TrimEnd('/');
}
