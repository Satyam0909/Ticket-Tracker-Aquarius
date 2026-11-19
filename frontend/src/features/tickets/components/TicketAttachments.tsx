import { useEffect, useState } from 'react'
import { ticketsApi } from '../api/ticketsApi'
import {
  allowedAttachmentExtensions,
  attachmentAccept,
  maxAttachmentSizeBytes,
  maxAttachmentsPerTicket,
  validateAttachmentFiles,
} from '../attachmentPolicy'
import type { TicketAttachment } from '../types'

function TicketAttachments({ ticketId, ticketTitle }: { ticketId: string; ticketTitle: string }) {
  const [isOpen, setIsOpen] = useState(false)
  const [attachments, setAttachments] = useState<TicketAttachment[]>([])
  const [selectedFiles, setSelectedFiles] = useState<File[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!isOpen) return

    let active = true
    ticketsApi
      .listAttachments(ticketId)
      .then((result) => {
        if (active) setAttachments(result)
      })
      .catch((cause: unknown) => {
        if (active) setError(cause instanceof Error ? cause.message : 'Unable to load attachments.')
      })
      .finally(() => {
        if (active) setIsLoading(false)
      })

    return () => {
      active = false
    }
  }, [isOpen, ticketId])

  function selectFiles(files: FileList | null) {
    if (!files) return
    const newFiles = [...selectedFiles, ...Array.from(files)]
    const validationError = validateAttachmentFiles(newFiles, attachments.length)
    if (validationError) setError(validationError)
    else {
      setSelectedFiles(newFiles)
      setError('')
    }
  }

  function toggleAttachments() {
    if (isOpen) {
      setIsOpen(false)
      return
    }

    setError('')
    setIsLoading(true)
    setIsOpen(true)
  }

  async function uploadFiles() {
    setIsUploading(true)
    setError('')
    try {
      const updatedAttachments = await ticketsApi.uploadAttachments(ticketId, selectedFiles)
      setAttachments(updatedAttachments)
      setSelectedFiles([])
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to upload attachments.')
    } finally {
      setIsUploading(false)
    }
  }

  async function downloadFile(attachment: TicketAttachment) {
    setError('')
    try {
      const file = await ticketsApi.downloadAttachment(ticketId, attachment.id)
      const url = URL.createObjectURL(file)
      const link = document.createElement('a')
      link.href = url
      link.download = attachment.fileName
      link.click()
      window.setTimeout(() => URL.revokeObjectURL(url), 0)
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to download attachment.')
    }
  }

  async function deleteFile(attachment: TicketAttachment) {
    setError('')
    try {
      await ticketsApi.deleteAttachment(ticketId, attachment.id)
      setAttachments((current) => current.filter((item) => item.id !== attachment.id))
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to delete attachment.')
    }
  }

  return (
    <div className="ticket-attachments">
      <button
        className="attachment-toggle"
        type="button"
        aria-expanded={isOpen}
        aria-label={`Attachments for ${ticketTitle}`}
        onClick={toggleAttachments}
      >
        Attachments ({attachments.length}/{maxAttachmentsPerTicket})
      </button>
      {isOpen && (
        <div className="attachment-manager">
          {isLoading ? (
            <span role="status">Loading attachments…</span>
          ) : (
            <ul className="attachment-list">
              {attachments.map((attachment) => (
                <li key={attachment.id}>
                  <button
                    className="attachment-download"
                    type="button"
                    onClick={() => void downloadFile(attachment)}
                  >
                    {attachment.fileName}
                  </button>
                  <span>{formatFileSize(attachment.length)}</span>
                  <button
                    className="attachment-remove"
                    type="button"
                    aria-label={`Delete ${attachment.fileName}`}
                    onClick={() => void deleteFile(attachment)}
                  >
                    Remove
                  </button>
                </li>
              ))}
            </ul>
          )}
          {attachments.length + selectedFiles.length < maxAttachmentsPerTicket && (
            <input
              className="attachment-input"
              type="file"
              aria-label="Add attachments"
              accept={attachmentAccept}
              multiple
              disabled={isUploading || isLoading}
              onChange={(event) => {
                selectFiles(event.currentTarget.files)
                event.currentTarget.value = ''
              }}
            />
          )}
          {selectedFiles.length > 0 && (
            <>
              <ul className="selected-attachments">
                {selectedFiles.map((file, index) => (
                  <li key={`${file.name}-${file.lastModified}-${index}`}>
                    <span>{file.name}</span>
                    <button
                      type="button"
                      aria-label={`Remove ${file.name}`}
                      onClick={() =>
                        setSelectedFiles((current) => current.filter((_, i) => i !== index))
                      }
                    >
                      Remove
                    </button>
                  </li>
                ))}
              </ul>
              <button
                className="attachment-upload-button"
                type="button"
                disabled={isUploading}
                onClick={() => void uploadFiles()}
              >
                {isUploading
                  ? 'Uploading…'
                  : `Upload ${selectedFiles.length} file${selectedFiles.length === 1 ? '' : 's'}`}
              </button>
            </>
          )}
          {error && (
            <p className="alert error-alert attachment-error" role="alert">
              {error}
            </p>
          )}
          <p className="attachment-hint">
            Maximum {maxAttachmentsPerTicket} files per ticket,{' '}
            {maxAttachmentSizeBytes / 1024 / 1024} MB each. Formats:{' '}
            {allowedAttachmentExtensions.join(', ')}
          </p>
        </div>
      )}
    </div>
  )
}

function formatFileSize(bytes: number): string {
  return bytes < 1024 * 1024
    ? `${Math.max(1, Math.ceil(bytes / 1024))} KB`
    : `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export default TicketAttachments
