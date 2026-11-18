using System.Net;
using System.Net.Http.Json;

namespace SupportTickets.Tests;

public sealed class AuthenticationApiTests
{
    [Fact]
    public async Task Ticket_endpoints_reject_requests_without_a_valid_account_token()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tickets?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registration_issues_a_token_that_authorizes_ticket_access()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new.user@example.test",
            password = "correct horse battery staple"
        });

        Assert.Equal(HttpStatusCode.OK, registration.StatusCode);
        var session = await registration.Content.ReadFromJsonAsync<AccountSessionForTests>();
        Assert.NotNull(session);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", session.AccessToken);

        var ticket = await client.PostAsJsonAsync("/api/tickets", new { title = "Authenticated ticket" });

        Assert.Equal(HttpStatusCode.Created, ticket.StatusCode);
    }

    [Fact]
    public async Task Registration_rejects_duplicate_email_and_login_uses_generic_failure()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();
        var credentials = new
        {
            email = "user@example.test",
            password = "correct horse battery staple"
        };
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/auth/register", credentials)).StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "USER@example.test",
            password = credentials.password
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var failedLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = credentials.email,
            password = "wrong password"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, failedLogin.StatusCode);
        Assert.DoesNotContain(credentials.email, await failedLogin.Content.ReadAsStringAsync());

        var successfulLogin = await client.PostAsJsonAsync("/api/auth/login", credentials);
        Assert.Equal(HttpStatusCode.OK, successfulLogin.StatusCode);
    }

    private sealed record AccountSessionForTests(string Email, string AccessToken, DateTime ExpiresAt);
}
