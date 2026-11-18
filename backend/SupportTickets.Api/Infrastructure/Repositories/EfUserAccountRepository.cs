using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Domain.Entities;
using SupportTickets.Api.Infrastructure.Persistence;

namespace SupportTickets.Api.Infrastructure.Repositories;

public sealed class EfUserAccountRepository(TicketDbContext dbContext) : IUserAccountRepository
{
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.UserAccounts.AnyAsync(account => account.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<UserAccount?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken) =>
        dbContext.UserAccounts.SingleOrDefaultAsync(
            account => account.NormalizedEmail == normalizedEmail,
            cancellationToken);

    public Task<RefreshToken?> FindRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .Include(token => token.Account)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public void Add(UserAccount account) => dbContext.UserAccounts.Add(account);

    public void Add(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
