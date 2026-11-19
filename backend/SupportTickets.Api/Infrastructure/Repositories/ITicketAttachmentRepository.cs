using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Infrastructure.Repositories;

public interface ITicketAttachmentRepository
{
    Task<bool> TicketExistsAsync(Guid ticketId, CancellationToken cancellationToken);
    Task<IReadOnlyList<int>> GetOccupiedSlotsAsync(Guid ticketId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketAttachmentResponse>> GetForTicketAsync(
        Guid ticketId,
        CancellationToken cancellationToken);
    Task<TicketAttachment?> GetByIdAsync(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken);
    Task AddRangeAsync(
        IReadOnlyCollection<TicketAttachment> attachments,
        CancellationToken cancellationToken);
    Task DeleteAsync(TicketAttachment attachment, CancellationToken cancellationToken);
}
