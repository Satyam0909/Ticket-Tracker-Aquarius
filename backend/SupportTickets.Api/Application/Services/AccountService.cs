using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
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
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

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
        var session = CreateSession(account);

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

        return session;
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

        var session = CreateSession(account);
        await accounts.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<AccountSession?> RefreshAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var refreshToken = await accounts.FindRefreshTokenAsync(HashToken(token), cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (refreshToken is null || refreshToken.RevokedAt.HasValue || refreshToken.ExpiresAt <= now)
            return null;

        refreshToken.RevokedAt = now;
        var session = CreateSession(refreshToken.Account);
        try
        {
            await accounts.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }

        return session;
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var refreshToken = await accounts.FindRefreshTokenAsync(HashToken(token), cancellationToken);
        if (refreshToken is null || refreshToken.RevokedAt.HasValue)
            return;

        refreshToken.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
        await accounts.SaveChangesAsync(cancellationToken);
    }

    private AccountSession CreateSession(UserAccount account)
    {
        var accessToken = tokenIssuer.Issue(account);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var refreshToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var refreshTokenExpiresAt = now.Add(RefreshTokenLifetime);
        accounts.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserAccountId = account.Id,
            Account = account,
            TokenHash = HashToken(refreshToken),
            CreatedAt = now,
            ExpiresAt = refreshTokenExpiresAt
        });

        return new AccountSession(
            account.Email,
            accessToken.Value,
            refreshToken,
            accessToken.ExpiresAt,
            refreshTokenExpiresAt);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
