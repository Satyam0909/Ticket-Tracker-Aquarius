using Microsoft.Extensions.Diagnostics.HealthChecks;
using SupportTickets.Api.Infrastructure.Persistence;

namespace SupportTickets.Api.Infrastructure.Health;

public sealed class DatabaseHealthCheck(TicketDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("The ticket database is unavailable.");
    }
}
