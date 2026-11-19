namespace SupportTickets.Api.Application.Contracts;

public sealed record TicketAttachmentUpload(string FileName, long Length, Func<Stream> OpenReadStream);

public sealed record TicketAttachmentResponse(
    Guid Id,
    string FileName,
    long Length,
    DateTime UploadedAt);

public sealed record TicketAttachmentDownload(string FileName, byte[] Content);
