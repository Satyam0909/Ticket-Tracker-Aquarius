using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Application.Contracts;

public sealed record RegisterAccountCommand(string Email, string Password);
public sealed record LoginCommand(string Email, string Password);
public sealed record IssuedAccessToken(string Value, DateTime ExpiresAt);
public sealed record AccountSession(
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt);

public interface IAccountService
{
    Task<AccountSession> RegisterAsync(RegisterAccountCommand command, CancellationToken cancellationToken);
    Task<AccountSession?> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AccountSession?> RefreshAsync(string token, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken);
}

public interface IUserAccountRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<UserAccount?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);
    void Add(UserAccount account);
    void Add(RefreshToken refreshToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(UserAccount account);
}
