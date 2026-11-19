using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
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
        var connectionString = environment.IsEnvironment("Testing")
            ? string.Empty
            : CreateSqlServerConnectionString(configuration, environment);

        services.AddDbContextPool<TicketDbContext>(options =>
            options.UseSqlServer(connectionString ?? string.Empty, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<ITicketAttachmentRepository, EfTicketAttachmentRepository>();
        services.AddScoped<IUserAccountRepository, EfUserAccountRepository>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("ticket-database", tags: ["ready"]);
        return services;
    }

    private static string CreateSqlServerConnectionString(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var dialect = RequiredSetting(configuration, "DB_DIALECT");
        if (!string.Equals(dialect, "mssql", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("DB_DIALECT must be 'mssql'.");

        var host = RequiredSetting(configuration, "DB_HOST");
        var database = RequiredSetting(configuration, "DB_DATABASE");
        var username = RequiredSetting(configuration, "DB_USERNAME");
        var password = RequiredSetting(configuration, "DB_PASSWORD", trim: false);
        var portValue = RequiredSetting(configuration, "DB_PORT");
        if (!int.TryParse(portValue, out var port) || port is < 1 or > 65535)
            throw new InvalidOperationException("DB_PORT must be a valid TCP port.");

        return new SqlConnectionStringBuilder
        {
            DataSource = $"{host},{port}",
            InitialCatalog = database,
            UserID = username,
            Password = password,
            Encrypt = true,
            TrustServerCertificate = environment.IsDevelopment()
        }.ConnectionString;
    }

    private static string RequiredSetting(IConfiguration configuration, string key, bool trim = true)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Configure {key}.");
        return trim ? value.Trim() : value;
    }
}
