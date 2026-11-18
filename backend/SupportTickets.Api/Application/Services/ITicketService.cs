using SupportTickets.Api.Application.Contracts;

namespace SupportTickets.Api.Application.Services;

public interface ITicketService
{
    Task<TicketPageResponse> GetTicketsAsync(TicketQuery query, CancellationToken cancellationToken);
    Task<TicketResponse> GetTicketAsync(Guid id, CancellationToken cancellationToken);
    Task<TicketResponse> CreateTicketAsync(CreateTicketCommand command, CancellationToken cancellationToken);
    Task<TicketResponse> UpdateTicketAsync(Guid id, UpdateTicketCommand command, CancellationToken cancellationToken);
    Task DeleteTicketAsync(Guid id, CancellationToken cancellationToken);
    Task<TicketSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}
