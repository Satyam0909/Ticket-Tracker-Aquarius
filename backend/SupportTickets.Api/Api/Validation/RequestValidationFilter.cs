using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportTickets.Api.Api.Constants;

namespace SupportTickets.Api.Api.Validation;

public sealed class RequestValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ModelState.IsValid)
        {
            await next();
            return;
        }

        var errors = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                _ => new[] { ApiMessages.InvalidRequest },
                StringComparer.OrdinalIgnoreCase);
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ApiMessages.ValidationFailed,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["code"] = ApiErrorCodes.ValidationFailed;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.Result = new BadRequestObjectResult(problem);
    }
}
