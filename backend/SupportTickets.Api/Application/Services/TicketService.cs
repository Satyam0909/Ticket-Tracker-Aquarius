using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Exceptions;
using SupportTickets.Api.Domain.Entities;
using SupportTickets.Api.Infrastructure.Repositories;

namespace SupportTickets.Api.Application.Services;

public sealed class TicketService(
    ITicketRepository repository,
    TimeProvider timeProvider) : ITicketService
{
    public async Task<TicketPageResponse> GetTicketsAsync(
        TicketQuery query,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetPageAsync(query, cancellationToken);
        return new TicketPageResponse(
            page.Items.Select(ToResponse).ToArray(),
            page.TotalCount,
            page.Page,
            page.PageSize);
    }

    public async Task<TicketResponse> GetTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, tracking: false, cancellationToken)
            ?? throw new TicketNotFoundException(id);
        return ToResponse(ticket);
    }

    public async Task<TicketResponse> CreateTicketAsync(
        CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Description = NormalizeDescription(command.Description),
            Status = command.Status,
            Priority = command.Priority,
            CreatedAt = now,
            UpdatedAt = now
        };

        await repository.AddAsync(ticket, cancellationToken);
        return ToResponse(ticket);
    }

    public async Task<TicketResponse> UpdateTicketAsync(
        Guid id,
        UpdateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, tracking: true, cancellationToken)
            ?? throw new TicketNotFoundException(id);

        ticket.Title = command.Title.Trim();
        ticket.Description = NormalizeDescription(command.Description);
        ticket.Status = command.Status;
        ticket.Priority = command.Priority;
        ticket.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await repository.SaveChangesAsync(cancellationToken);
        return ToResponse(ticket);
    }

    public async Task DeleteTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, tracking: true, cancellationToken)
            ?? throw new TicketNotFoundException(id);

        await repository.DeleteAsync(ticket, cancellationToken);
    }

    public Task<TicketSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken) =>
        repository.GetSummaryAsync(cancellationToken);

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static TicketResponse ToResponse(Ticket ticket) =>
        new(ticket.Id, ticket.Title, ticket.Description, ticket.Status, ticket.Priority,
            ticket.CreatedAt, ticket.UpdatedAt);
}
