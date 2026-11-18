using System.ComponentModel.DataAnnotations;

namespace SupportTickets.Api.Api.Contracts;

public sealed class RegisterAccountRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public required string Email { get; init; }

    [Required]
    [StringLength(128, MinimumLength = 12)]
    public required string Password { get; init; }
}

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public required string Email { get; init; }

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public required string Password { get; init; }
}

public sealed record AccountSessionResponse(string Email, DateTime ExpiresAt);
