using System.Linq.Expressions;
using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Application.Utilities;

public static class TicketQueryExtensions
{
    public static IQueryable<Ticket> ApplyFilters(this IQueryable<Ticket> query, string? status, string? search)
    {
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(ticket => ticket.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var title = search.Trim().ToLower();
            query = query.Where(ticket => ticket.Title.ToLower().Contains(title));
        }

        return query;
    }

    public static IOrderedQueryable<Ticket> ApplySorting(
        this IQueryable<Ticket> query,
        string sortBy,
        string sortDirection)
    {
        var ascending = sortDirection == "asc";
        IOrderedQueryable<Ticket> ordered = sortBy switch
        {
            "title" => Order(query, ticket => ticket.Title, ascending),
            "status" => Order(query, ticket => ticket.Status, ascending),
            "priority" => Order(query, ticket => ticket.Priority, ascending),
            _ => Order(query, ticket => ticket.CreatedAt, ascending)
        };
        return ordered.ThenBy(ticket => ticket.Id);
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var offset = checked((int)((long)(page - 1) * pageSize));
        return query.Skip(offset).Take(pageSize);
    }

    private static IOrderedQueryable<Ticket> Order<TKey>(
        IQueryable<Ticket> query,
        Expression<Func<Ticket, TKey>> keySelector,
        bool ascending) =>
        ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
}
