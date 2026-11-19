using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Application.Exceptions;
using SupportTickets.Api.Domain.Entities;
using SupportTickets.Api.Infrastructure.Repositories;

namespace SupportTickets.Api.Application.Services;

public sealed class TicketAttachmentService(
    ITicketAttachmentRepository repository,
    TimeProvider timeProvider) : ITicketAttachmentService
{
    private const int MaxAttachmentsPerTicket = 3;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxFileNameLength = 255;

    private const string AllowedExtensionsMessage =
        "Only .jpg, .jpeg, .png, .tiff, .pdf, .msg, .doc, .docx, .txt, .csv, .xls, .xlsx, .xlsm, .xltx, .xltm, .ppt, .pptx, .zip, .rar, .igs, .stp, .step, .x_t, .catpart, .dwg, .stl, .sla, .dxf, and .jt files are allowed.";

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".js", ".mjs", ".cjs", ".php", ".exe", ".sh", ".bat"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".tiff", ".pdf", ".msg", ".doc", ".docx", ".txt", ".csv",
        ".xls", ".xlsx", ".xlsm", ".xltx", ".xltm", ".ppt", ".pptx", ".zip", ".rar", ".igs",
        ".stp", ".step", ".x_t", ".catpart", ".dwg", ".stl", ".sla", ".dxf", ".jt"
    };

    public async Task<IReadOnlyList<TicketAttachmentResponse>> GetForTicketAsync(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        await EnsureTicketExistsAsync(ticketId, cancellationToken);
        return await repository.GetForTicketAsync(ticketId, cancellationToken);
    }

    public async Task<IReadOnlyList<TicketAttachmentResponse>> UploadAsync(
        Guid ticketId,
        IReadOnlyList<TicketAttachmentUpload> files,
        CancellationToken cancellationToken)
    {
        if (files.Count is < 1 or > MaxAttachmentsPerTicket)
            throw new InvalidTicketAttachmentException("Select between 1 and 3 files per upload.");

        var normalizedFiles = files.Select(NormalizeAndValidate).ToArray();
        await EnsureTicketExistsAsync(ticketId, cancellationToken);

        var occupiedSlots = await repository.GetOccupiedSlotsAsync(ticketId, cancellationToken);
        var availableSlots = Enumerable.Range(1, MaxAttachmentsPerTicket)
            .Except(occupiedSlots)
            .Take(files.Count)
            .ToArray();
        if (availableSlots.Length != files.Count)
            throw new TicketAttachmentLimitExceededException();

        var attachments = new List<TicketAttachment>(files.Count);
        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            using var source = file.OpenReadStream();
            using var buffer = new MemoryStream((int)file.Length);
            await source.CopyToAsync(buffer, cancellationToken);
            if (buffer.Length != file.Length)
                throw new InvalidTicketAttachmentException("The uploaded file could not be read completely.");

            attachments.Add(new TicketAttachment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                Slot = availableSlots[index],
                FileName = normalizedFiles[index],
                Length = buffer.Length,
                Content = buffer.ToArray(),
                UploadedAt = timeProvider.GetUtcNow().UtcDateTime
            });
        }

        await repository.AddRangeAsync(attachments, cancellationToken);
        return await repository.GetForTicketAsync(ticketId, cancellationToken);
    }

    public async Task<TicketAttachmentDownload> DownloadAsync(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        await EnsureTicketExistsAsync(ticketId, cancellationToken);
        var attachment = await repository.GetByIdAsync(ticketId, attachmentId, cancellationToken);
        if (attachment is null)
            throw new TicketAttachmentNotFoundException();

        return new TicketAttachmentDownload(attachment.FileName, attachment.Content);
    }

    public async Task DeleteAsync(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken)
    {
        await EnsureTicketExistsAsync(ticketId, cancellationToken);
        var attachment = await repository.GetByIdAsync(ticketId, attachmentId, cancellationToken);
        if (attachment is null)
            throw new TicketAttachmentNotFoundException();

        await repository.DeleteAsync(attachment, cancellationToken);
    }

    private async Task EnsureTicketExistsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        if (!await repository.TicketExistsAsync(ticketId, cancellationToken))
            throw new TicketNotFoundException(ticketId);
    }

    private static string NormalizeAndValidate(TicketAttachmentUpload file)
    {
        if (file.Length <= 0)
            throw new InvalidTicketAttachmentException("Empty files cannot be uploaded.");
        if (file.Length > MaxFileSizeBytes)
            throw new TicketAttachmentTooLargeException();

        var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > MaxFileNameLength)
            throw new InvalidTicketAttachmentException("The file name is invalid or too long.");

        var extension = Path.GetExtension(fileName);
        if (BlockedExtensions.Contains(extension) || !AllowedExtensions.Contains(extension))
            throw new InvalidTicketAttachmentException(AllowedExtensionsMessage);

        return fileName;
    }
}
