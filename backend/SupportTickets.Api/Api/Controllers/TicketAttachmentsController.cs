using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Services;

namespace SupportTickets.Api.Api.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("tickets-api")]
[Route("api/tickets/{ticketId:guid}/attachments")]
public sealed class TicketAttachmentsController(ITicketAttachmentService attachments) : ControllerBase
{
    private const int MaxRequestSizeBytes = 32 * 1024 * 1024;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketAttachmentResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketAttachmentResponse>>> GetForTicket(
        Guid ticketId,
        CancellationToken cancellationToken) =>
        Ok(await attachments.GetForTicketAsync(ticketId, cancellationToken));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestSizeBytes)]
    [ProducesResponseType<IReadOnlyList<TicketAttachmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<IReadOnlyList<TicketAttachmentResponse>>> Upload(
        Guid ticketId,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var uploads = files
            .Select(file => new TicketAttachmentUpload(file.FileName, file.Length, file.OpenReadStream))
            .ToArray();
        return Ok(await attachments.UploadAsync(ticketId, uploads, cancellationToken));
    }

    [HttpGet("{attachmentId:guid}")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await attachments.DownloadAsync(ticketId, attachmentId, cancellationToken);
        return File(attachment.Content, "application/octet-stream", attachment.FileName);
    }

    [HttpDelete("{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        await attachments.DeleteAsync(ticketId, attachmentId, cancellationToken);
        return NoContent();
    }
}
