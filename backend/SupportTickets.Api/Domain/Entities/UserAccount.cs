namespace SupportTickets.Api.Domain.Entities;

public sealed class UserAccount
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
