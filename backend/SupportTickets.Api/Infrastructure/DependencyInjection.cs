using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Services;
using SupportTickets.Api.Domain.Entities;
using SupportTickets.Api.Infrastructure.Health;
using SupportTickets.Api.Infrastructure.Persistence;
using SupportTickets.Api.Infrastructure.Repositories;

namespace SupportTickets.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Tickets");
        if (environment.IsProduction() && string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure the ConnectionStrings:Tickets setting.");

        services.AddDbContextPool<TicketDbContext>(options =>
            options.UseSqlServer(connectionString ?? string.Empty, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<IUserAccountRepository, EfUserAccountRepository>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("ticket-database", tags: ["ready"]);
        return services;
    }
}
