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

    public void Add(UserAccount account) => dbContext.UserAccounts.Add(account);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
