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
    public async Task Authentication_write_requests_reject_untrusted_origins()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://attacker.example");

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "blocked@example.test",
            password = "correct horse battery staple"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Registration_sets_http_only_cookies_that_authorize_ticket_access()
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
        Assert.False(string.IsNullOrWhiteSpace(session.Email));
        Assert.DoesNotContain("accessToken", await registration.Content.ReadAsStringAsync());
        Assert.DoesNotContain("refreshToken", await registration.Content.ReadAsStringAsync());
        Assert.Contains(registration.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.Contains("ticket_tracker_access=", StringComparison.Ordinal) &&
            cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(registration.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.Contains("ticket_tracker_refresh=", StringComparison.Ordinal) &&
            cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        SupportTicketsApiFactory.SetAuthCookies(client, registration);

        var ticket = await client.PostAsJsonAsync("/api/tickets", new { title = "Authenticated ticket" });

        Assert.Equal(HttpStatusCode.Created, ticket.StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_tokens_and_rejects_reuse_of_the_previous_refresh_token()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "rotate@example.test",
            password = "correct horse battery staple"
        });
        var oldRefreshToken = SupportTicketsApiFactory.GetCookieValue(
            registration,
            "ticket_tracker_refresh");
        Assert.NotNull(oldRefreshToken);
        SupportTicketsApiFactory.SetAuthCookies(client, registration);

        var refresh = await client.PostAsync("/api/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var newRefreshToken = SupportTicketsApiFactory.GetCookieValue(refresh, "ticket_tracker_refresh");
        Assert.NotNull(newRefreshToken);
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        using var replayClient = factory.CreateClient();
        replayClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "Cookie",
            $"ticket_tracker_refresh={oldRefreshToken}");
        var replay = await replayClient.PostAsync("/api/auth/refresh", content: null);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_refresh_token_and_clears_cookies()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "logout@example.test",
            password = "correct horse battery staple"
        });
        SupportTicketsApiFactory.SetAuthCookies(client, registration);

        var logout = await client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Contains(logout.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.Contains("ticket_tracker_refresh=", StringComparison.Ordinal) &&
            cookie.Contains("expires=", StringComparison.OrdinalIgnoreCase));
        var refresh = await client.PostAsync("/api/auth/refresh", content: null);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
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

    private sealed record AccountSessionForTests(string Email, DateTime ExpiresAt);
}
