using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Infrastructure.Repositories;

public interface ITicketRepository
{
    Task<PageResult<Ticket>> GetPageAsync(TicketQuery query, CancellationToken cancellationToken);
    Task<Ticket?> GetByIdAsync(Guid id, bool tracking, CancellationToken cancellationToken);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<TicketSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}
