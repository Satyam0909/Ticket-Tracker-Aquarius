namespace SupportTickets.Api.Application.Exceptions;

public sealed class InvalidTicketAttachmentException(string message) : Exception(message)
{
}

public sealed class TicketAttachmentTooLargeException()
    : Exception("Each attachment must be 10 MB or smaller.")
{
}

public sealed class TicketAttachmentLimitExceededException()
    : Exception("A ticket can have no more than 3 attachments.")
{
}

public sealed class TicketAttachmentSlotConflictException()
    : Exception("Attachments changed at the same time. Refresh the list and try again.")
{
}

public sealed class TicketAttachmentNotFoundException()
    : Exception("The requested attachment was not found.")
{
}
