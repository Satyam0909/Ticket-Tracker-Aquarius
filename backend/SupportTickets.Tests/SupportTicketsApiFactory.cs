using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SupportTickets.Api.Infrastructure.Persistence;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SupportTickets.Tests;

public sealed class SupportTicketsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var databaseName = $"ticket-tests-{Guid.NewGuid():N}";
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
                ["RateLimiting:PermitLimit"] = "1000"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<TicketDbContext>>();
            services.RemoveAll<DbContextOptions<TicketDbContext>>();
            services.RemoveAll<TicketDbContext>();
            services.AddDbContext<TicketDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{Guid.NewGuid():N}@example.test",
            password = "correct horse battery staple"
        });
        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<AccountSessionForTests>();
        Assert.NotNull(session);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            session.AccessToken);
        return client;
    }

    private sealed record AccountSessionForTests(string Email, string AccessToken, DateTime ExpiresAt);
}
