import { useState, type FormEvent } from 'react'
import {
  ticketPriorities,
  ticketStatuses,
  type TicketPriority,
  type TicketStatus,
} from '../../../shared/constants/ticketOptions'
import type { TicketDraft } from '../types'

interface TicketFormProps {
  onCreate: (draft: TicketDraft) => Promise<void>
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

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
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
      await onCreate({ ...draft, title })
      setDraft(initialDraft)
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to create ticket.')
    } finally {
      setIsSaving(false)
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
        {error && (
          <div className="alert error-alert" role="alert">
            {error}
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
