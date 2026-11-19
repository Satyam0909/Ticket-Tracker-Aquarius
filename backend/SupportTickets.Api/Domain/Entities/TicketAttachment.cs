namespace SupportTickets.Api.Domain.Entities;

public sealed class TicketAttachment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public int Slot { get; set; }
    public required string FileName { get; set; }
    public long Length { get; set; }
    public required byte[] Content { get; set; }
    public DateTime UploadedAt { get; set; }
}
