using System.ComponentModel.DataAnnotations;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Api.Validation;

namespace SupportTickets.Api.Api.Contracts;

public sealed class CreateTicketRequest
{
    [Required, StringLength(120, MinimumLength = 1)]
    public string Title { get; init; } = "";

    [StringLength(2000)]
    public string? Description { get; init; }

    [Required, AllowedValue("open", "in_progress", "closed")]
    public string Status { get; init; } = "open";

    [Required, AllowedValue("low", "medium", "high")]
    public string Priority { get; init; } = "medium";

    public CreateTicketCommand ToCommand() => new(Title, Description, Status, Priority);
}

public sealed class UpdateTicketRequest
{
    [Required, StringLength(120, MinimumLength = 1)]
    public string Title { get; init; } = "";

    [StringLength(2000)]
    public string? Description { get; init; }

    [Required, AllowedValue("open", "in_progress", "closed")]
    public string Status { get; init; } = "";

    [Required, AllowedValue("low", "medium", "high")]
    public string Priority { get; init; } = "";

    public UpdateTicketCommand ToCommand() => new(Title, Description, Status, Priority);
}

public sealed class TicketListRequest : IValidatableObject
{
    [AllowedValue("open", "in_progress", "closed")]
    public string? Status { get; init; }

    [StringLength(120)]
    public string? Search { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [AllowedValue("createdAt", "title", "status", "priority")]
    public string SortBy { get; init; } = "createdAt";

    [AllowedValue("asc", "desc")]
    public string SortDirection { get; init; } = "desc";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Page > 1 && (long)(Page - 1) * PageSize > int.MaxValue)
            yield return new ValidationResult("Page is too large.", [nameof(Page)]);
    }

    public TicketQuery ToQuery() =>
        new(Status, Search, Page, PageSize, SortBy, SortDirection);
}
