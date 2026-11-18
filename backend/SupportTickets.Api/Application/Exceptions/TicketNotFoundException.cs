namespace SupportTickets.Api.Application.Exceptions;

public sealed class TicketNotFoundException(Guid id)
    : Exception($"Ticket '{id}' was not found.")
{
    public Guid TicketId { get; } = id;
}
