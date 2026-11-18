using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Application.Contracts;

public sealed record CreateTicketCommand(string Title, string? Description, string Status, string Priority);

public sealed record UpdateTicketCommand(string Title, string? Description, string Status, string Priority);

public sealed record TicketQuery(
    string? Status,
    string? Search,
    int Page,
    int PageSize,
    string SortBy,
    string SortDirection);

public sealed record PageResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TicketPageResponse(
    IReadOnlyList<TicketResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record CountByValue(string Value, int Count);

public sealed record TicketSummaryResponse(
    IReadOnlyList<CountByValue> ByStatus,
    IReadOnlyList<CountByValue> ByPriority);
