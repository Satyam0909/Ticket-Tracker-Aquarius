import { useState, type FormEvent } from 'react'
import {
  ticketPriorities,
  ticketStatuses,
  type TicketPriority,
  type TicketStatus,
} from '../../../shared/constants/ticketOptions'
import {
  allowedAttachmentExtensions,
  attachmentAccept,
  maxAttachmentSizeBytes,
  maxAttachmentsPerTicket,
  validateAttachmentFiles,
} from '../attachmentPolicy'
import type { TicketDraft } from '../types'

interface TicketFormProps {
  onCreate: (draft: TicketDraft, files: File[]) => Promise<void>
}

const initialDraft: TicketDraft = {
  title: '',
  description: '',
  status: 'open',
  priority: 'medium',
}

function TicketForm({ onCreate }: TicketFormProps) {
  const [draft, setDraft] = useState<TicketDraft>(initialDraft)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const [attachmentError, setAttachmentError] = useState('')
  const [files, setFiles] = useState<File[]>([])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (attachmentError) {
      setError(attachmentError)
      return
    }

    const title = draft.title.trim()
    if (!title) {
      setError('Enter a ticket title.')
      return
    }
    if (title.length > 120) {
      setError('Title must be 120 characters or fewer.')
      return
    }
    if (draft.description.length > 2000) {
      setError('Description must be 2000 characters or fewer.')
      return
    }

    setIsSaving(true)
    setError('')
    try {
      await onCreate({ ...draft, title }, files)
      setDraft(initialDraft)
      setFiles([])
      setAttachmentError('')
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to create ticket.')
    } finally {
      setIsSaving(false)
    }
  }

  function selectFiles(selected: FileList | null) {
    if (!selected) return
    const newFiles = [...files, ...Array.from(selected)]
    const validationError = validateAttachmentFiles(newFiles)
    if (validationError) setAttachmentError(validationError)
    else {
      setFiles(newFiles)
      setAttachmentError('')
      setError('')
    }
  }

  return (
    <aside className="create-panel">
      <div className="create-heading">
        <span className="create-icon">＋</span>
        <div>
          <p className="eyebrow">ADD TO THE QUEUE</p>
          <h2>New ticket</h2>
        </div>
      </div>
      <form onSubmit={handleSubmit} noValidate>
        <label className="form-label" htmlFor="title">
          Ticket title <span>*</span>
        </label>
        <input
          id="title"
          className="form-control"
          value={draft.title}
          maxLength={120}
          placeholder="What needs attention?"
          onChange={(event) => setDraft({ ...draft, title: event.target.value })}
        />
        <div className="field-hint">
          <span>Keep it clear and concise</span>
          <span>{draft.title.length}/120</span>
        </div>
        <label className="form-label" htmlFor="description">
          Description <span className="optional">(optional)</span>
        </label>
        <textarea
          id="description"
          className="form-control description-control"
          value={draft.description}
          maxLength={2000}
          placeholder="Add helpful details..."
          onChange={(event) => setDraft({ ...draft, description: event.target.value })}
        />
        <div className="field-hint">
          <span>Include relevant context</span>
          <span>{draft.description.length}/2000</span>
        </div>
        <div className="form-row">
          <div>
            <label className="form-label" htmlFor="priority">
              Priority
            </label>
            <select
              id="priority"
              className="form-control"
              value={draft.priority}
              onChange={(event) =>
                setDraft({ ...draft, priority: event.target.value as TicketPriority })
              }
            >
              {ticketPriorities.map((value) => (
                <option key={value} value={value}>
                  {value[0].toUpperCase() + value.slice(1)}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="form-label" htmlFor="new-status">
              Status
            </label>
            <select
              id="new-status"
              className="form-control"
              value={draft.status}
              onChange={(event) =>
                setDraft({ ...draft, status: event.target.value as TicketStatus })
              }
            >
              {ticketStatuses.map((value) => (
                <option key={value} value={value}>
                  {value.replace('_', ' ')}
                </option>
              ))}
            </select>
          </div>
        </div>
        <label className="form-label" htmlFor="ticket-attachments">
          Attachments <span className="optional">(optional)</span>
        </label>
        <input
          id="ticket-attachments"
          className="form-control attachment-input"
          type="file"
          accept={attachmentAccept}
          multiple
          disabled={isSaving}
          onChange={(event) => {
            selectFiles(event.currentTarget.files)
            event.currentTarget.value = ''
          }}
        />
        <p className="attachment-hint">
          Up to {maxAttachmentsPerTicket} files, {maxAttachmentSizeBytes / 1024 / 1024} MB each.
          Allowed: {allowedAttachmentExtensions.join(', ')}
        </p>
        {files.length > 0 && (
          <ul className="selected-attachments">
            {files.map((file, index) => (
              <li key={`${file.name}-${file.lastModified}-${index}`}>
                <span>{file.name}</span>
                <button
                  type="button"
                  aria-label={`Remove ${file.name}`}
                  onClick={() => {
                    setFiles(files.filter((_, fileIndex) => fileIndex !== index))
                    setAttachmentError('')
                    setError('')
                  }}
                >
                  Remove
                </button>
              </li>
            ))}
          </ul>
        )}
        {(error || attachmentError) && (
          <div className="alert error-alert" role="alert">
            {error || attachmentError}
          </div>
        )}
        <button className="submit-button" type="submit" disabled={isSaving}>
          {isSaving ? 'Creating…' : 'Create ticket'} <span>→</span>
        </button>
      </form>
      <p className="form-note">Your team will be able to pick this up right away.</p>
    </aside>
  )
}

export default TicketForm
