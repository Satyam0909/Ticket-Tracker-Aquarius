using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SupportTickets.Api.Infrastructure.Persistence;
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
        SetAuthCookies(client, response);
        return client;
    }

    public static void SetAuthCookies(HttpClient client, HttpResponseMessage response)
    {
        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(header => header.Split(';', 2)[0]);
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", string.Join("; ", cookies));
    }

    public static string? GetCookieValue(HttpResponseMessage response, string name) =>
        response.Headers.GetValues("Set-Cookie")
            .Select(header => header.Split(';', 2)[0])
            .Select(cookie => cookie.Split('=', 2))
            .Where(parts => parts.Length == 2 && parts[0] == name)
            .Select(parts => parts[1])
            .FirstOrDefault();
}
