namespace SupportTickets.Api.Api.Constants;

public static class ApiErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string TicketNotFound = "ticket_not_found";
    public const string AccountAlreadyExists = "account_already_exists";
    public const string RateLimitExceeded = "rate_limit_exceeded";
    public const string InvalidTicketAttachment = "invalid_ticket_attachment";
    public const string TicketAttachmentTooLarge = "ticket_attachment_too_large";
    public const string TicketAttachmentLimitExceeded = "ticket_attachment_limit_exceeded";
    public const string TicketAttachmentConflict = "ticket_attachment_conflict";
    public const string TicketAttachmentNotFound = "ticket_attachment_not_found";
    public const string InternalError = "internal_error";
}

public static class ApiMessages
{
    public const string InvalidRequest = "The submitted value is invalid.";
    public const string ValidationFailed = "One or more validation errors occurred.";
    public const string TicketNotFound = "The requested ticket was not found.";
    public const string AccountAlreadyExists = "An account with that email address already exists.";
    public const string RateLimitExceeded = "Too many requests. Please try again later.";
    public const string InternalError = "An unexpected error occurred.";
}
