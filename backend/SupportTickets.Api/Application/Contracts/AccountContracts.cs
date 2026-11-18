using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Application.Contracts;

public sealed record RegisterAccountCommand(string Email, string Password);
public sealed record LoginCommand(string Email, string Password);
public sealed record IssuedAccessToken(string Value, DateTime ExpiresAt);
public sealed record AccountSession(string Email, string AccessToken, DateTime ExpiresAt);

public interface IAccountService
{
    Task<AccountSession> RegisterAsync(RegisterAccountCommand command, CancellationToken cancellationToken);
    Task<AccountSession?> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}

public interface IUserAccountRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<UserAccount?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    void Add(UserAccount account);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(UserAccount account);
}
