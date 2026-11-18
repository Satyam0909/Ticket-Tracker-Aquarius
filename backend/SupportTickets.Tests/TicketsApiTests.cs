using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SupportTickets.Api.Application.Contracts;

namespace SupportTickets.Tests;

public sealed class TicketsApiTests
{
    [Fact]
    public async Task Create_ticket_returns_resource_and_can_be_listed()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = await factory.CreateAuthenticatedClientAsync();

        var created = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Cannot sign in",
            description = "Reset flow fails",
            status = "open",
            priority = "high"
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var ticket = await created.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(ticket);
        Assert.Equal("Cannot sign in", ticket.Title);

        var list = await client.GetFromJsonAsync<JsonElement>("/api/tickets?page=1&pageSize=10");
        Assert.Equal(1, list.GetProperty("totalCount").GetInt32());
        Assert.Equal(ticket.Id, list.GetProperty("items")[0].GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Invalid_request_returns_problem_details_without_echoing_input()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = await factory.CreateAuthenticatedClientAsync();
        const string invalidStatus = "unsupported-status";

        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Valid title",
            status = invalidStatus
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("validation_failed", body);
        Assert.DoesNotContain(invalidStatus, body);
    }

    [Fact]
    public async Task List_filters_and_valid_missing_ids_return_expected_status_codes()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = await factory.CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/tickets", new { title = "Printer offline", status = "open" });
        await client.PostAsJsonAsync("/api/tickets", new { title = "Printer supplies", status = "closed" });

        var filtered = await client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?status=open&search=printer&page=1&pageSize=10");
        Assert.Equal(1, filtered.GetProperty("totalCount").GetInt32());

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/tickets/summary");
        var openCount = summary.GetProperty("byStatus")
            .EnumerateArray()
            .Single(item => item.GetProperty("value").GetString() == "open")
            .GetProperty("count")
            .GetInt32();
        Assert.Equal(1, openCount);

        var missing = await client.GetAsync($"/api/tickets/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Contains("ticket_not_found", await missing.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/tickets/not-a-guid")).StatusCode);
    }

    [Fact]
    public async Task Update_and_delete_preserve_resource_lifecycle()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = await factory.CreateAuthenticatedClientAsync();
        var createResponse = await client.PostAsJsonAsync("/api/tickets", new { title = "Old title" });
        var created = await createResponse.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync($"/api/tickets/{created.Id}", new
        {
            title = "Updated title",
            description = (string?)null,
            status = "closed",
            priority = "high"
        });
        var updated = await updateResponse.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("closed", updated?.Status);

        var deleteResponse = await client.DeleteAsync($"/api/tickets/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/tickets/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Liveness_endpoint_is_available()
    {
        using var factory = new SupportTicketsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
