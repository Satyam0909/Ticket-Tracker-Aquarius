using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Exceptions;
using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Application.Services;

public sealed class AccountService(
    IUserAccountRepository accounts,
    IPasswordHasher<UserAccount> passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    TimeProvider timeProvider) : IAccountService
{
    public async Task<AccountSession> RegisterAsync(
        RegisterAccountCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        if (await accounts.EmailExistsAsync(normalizedEmail, cancellationToken))
            throw new AccountAlreadyExistsException();

        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = command.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = string.Empty,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        account.PasswordHash = passwordHasher.HashPassword(account, command.Password);
        accounts.Add(account);

        try
        {
            await accounts.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await accounts.EmailExistsAsync(normalizedEmail, cancellationToken))
                throw new AccountAlreadyExistsException();
            throw;
        }

        return CreateSession(account);
    }

    public async Task<AccountSession?> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var account = await accounts.FindByNormalizedEmailAsync(
            NormalizeEmail(command.Email),
            cancellationToken);
        if (account is null ||
            passwordHasher.VerifyHashedPassword(account, account.PasswordHash, command.Password)
                == PasswordVerificationResult.Failed)
            return null;

        return CreateSession(account);
    }

    private AccountSession CreateSession(UserAccount account)
    {
        var token = tokenIssuer.Issue(account);
        return new AccountSession(account.Email, token.Value, token.ExpiresAt);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
