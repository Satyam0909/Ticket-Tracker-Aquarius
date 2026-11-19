using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Exceptions;
using SupportTickets.Api.Domain.Entities;
using SupportTickets.Api.Infrastructure.Persistence;

namespace SupportTickets.Api.Infrastructure.Repositories;

public sealed class EfTicketAttachmentRepository(TicketDbContext db) : ITicketAttachmentRepository
{
    public Task<bool> TicketExistsAsync(Guid ticketId, CancellationToken cancellationToken) =>
        db.Tickets.AnyAsync(ticket => ticket.Id == ticketId, cancellationToken);

    public async Task<IReadOnlyList<int>> GetOccupiedSlotsAsync(
        Guid ticketId,
        CancellationToken cancellationToken) =>
        await db.TicketAttachments
            .AsNoTracking()
            .Where(attachment => attachment.TicketId == ticketId)
            .Select(attachment => attachment.Slot)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TicketAttachmentResponse>> GetForTicketAsync(
        Guid ticketId,
        CancellationToken cancellationToken) =>
        await db.TicketAttachments
            .AsNoTracking()
            .Where(attachment => attachment.TicketId == ticketId)
            .OrderBy(attachment => attachment.Slot)
            .Select(attachment => new TicketAttachmentResponse(
                attachment.Id,
                attachment.FileName,
                attachment.Length,
                attachment.UploadedAt))
            .ToListAsync(cancellationToken);

    public Task<TicketAttachment?> GetByIdAsync(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken) =>
        db.TicketAttachments.SingleOrDefaultAsync(
            attachment => attachment.TicketId == ticketId && attachment.Id == attachmentId,
            cancellationToken);

    public async Task AddRangeAsync(
        IReadOnlyCollection<TicketAttachment> attachments,
        CancellationToken cancellationToken)
    {
        db.TicketAttachments.AddRange(attachments);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueIndexViolation(exception))
        {
            throw new TicketAttachmentSlotConflictException();
        }
    }

    public async Task DeleteAsync(TicketAttachment attachment, CancellationToken cancellationToken)
    {
        db.TicketAttachments.Remove(attachment);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueIndexViolation(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
