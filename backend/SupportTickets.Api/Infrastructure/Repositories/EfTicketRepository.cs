using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Utilities;
using SupportTickets.Api.Domain.Entities;
using SupportTickets.Api.Infrastructure.Persistence;

namespace SupportTickets.Api.Infrastructure.Repositories;

public sealed class EfTicketRepository(TicketDbContext db) : ITicketRepository
{
    public async Task<PageResult<Ticket>> GetPageAsync(
        TicketQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Tickets.AsNoTracking().ApplyFilters(request.Status, request.Search);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .ApplySorting(request.SortBy, request.SortDirection)
            .ApplyPagination(request.Page, request.PageSize)
            .ToListAsync(cancellationToken);

        return new PageResult<Ticket>(items, totalCount, request.Page, request.PageSize);
    }

    public Task<Ticket?> GetByIdAsync(Guid id, bool tracking, CancellationToken cancellationToken)
    {
        IQueryable<Ticket> query = db.Tickets;
        if (!tracking)
            query = query.AsNoTracking();
        return query.SingleOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await db.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TicketSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var statuses = await db.Tickets.AsNoTracking()
            .GroupBy(ticket => ticket.Status)
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .OrderBy(item => item.Value)
            .ToListAsync(cancellationToken);
        var priorities = await db.Tickets.AsNoTracking()
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .OrderBy(item => item.Value)
            .ToListAsync(cancellationToken);

        return new TicketSummaryResponse(
            statuses.Select(item => new CountByValue(item.Value, item.Count)).ToArray(),
            priorities.Select(item => new CountByValue(item.Value, item.Count)).ToArray());
    }
}
