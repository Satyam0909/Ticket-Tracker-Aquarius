using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportTickets.Api.Api.Contracts;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Services;

namespace SupportTickets.Api.Api.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("tickets-api")]
[Route("api/tickets")]
public sealed class TicketsController(ITicketService tickets) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TicketPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketPageResponse>> GetTickets(
        [FromQuery] TicketListRequest request,
        CancellationToken cancellationToken) =>
        Ok(await tickets.GetTicketsAsync(request.ToQuery(), cancellationToken));

    [HttpGet("{id}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetTicketById(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await tickets.GetTicketAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponse>> CreateTicket(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await tickets.CreateTicketAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> UpdateTicket(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken) =>
        Ok(await tickets.UpdateTicketAsync(id, request.ToCommand(), cancellationToken));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket(Guid id, CancellationToken cancellationToken)
    {
        await tickets.DeleteTicketAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("summary")]
    [ProducesResponseType<TicketSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketSummaryResponse>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await tickets.GetSummaryAsync(cancellationToken));
}
