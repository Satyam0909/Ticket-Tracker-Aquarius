using SupportTickets.Api.Application.Contracts;

namespace SupportTickets.Api.Application.Services;

public interface ITicketAttachmentService
{
    Task<IReadOnlyList<TicketAttachmentResponse>> GetForTicketAsync(
        Guid ticketId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TicketAttachmentResponse>> UploadAsync(
        Guid ticketId,
        IReadOnlyList<TicketAttachmentUpload> files,
        CancellationToken cancellationToken);

    Task<TicketAttachmentDownload> DownloadAsync(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken);
}
