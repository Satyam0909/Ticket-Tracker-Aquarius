import {
  ticketPriorities,
  ticketStatuses,
  type TicketStatus,
} from '../../../shared/constants/ticketOptions'
import type { CountByValue, TicketSummary as TicketSummaryModel } from '../types'

interface TicketSummaryProps {
  summary: TicketSummaryModel
  statusFilter: TicketStatus | ''
  onStatusSelect(status: TicketStatus | ''): void
}

function countFor(items: CountByValue[], value: string): number {
  return items.find((item) => item.value === value)?.count ?? 0
}

function TicketSummary({ summary, statusFilter, onStatusSelect }: TicketSummaryProps) {
  const totalTickets = summary.byStatus.reduce((total, item) => total + item.count, 0)
  const priorityTotal = summary.byPriority.reduce((total, item) => total + item.count, 0)

  return (
    <section className="summary-grid" aria-label="Ticket summary">
      <button
        className={`summary-card summary-filter ${statusFilter === '' ? 'is-selected' : ''}`}
        type="button"
        aria-label="Show all tickets"
        aria-pressed={statusFilter === ''}
        onClick={() => onStatusSelect('')}
      >
        <span className="summary-icon icon-lilac" aria-hidden="true">
          ▤
        </span>
        <div>
          <p>All tickets</p>
          <strong>{totalTickets}</strong>
        </div>
        <span className="summary-note">total</span>
      </button>
      {ticketStatuses.map((status) => (
        <button
          className={`summary-card summary-filter ${statusFilter === status ? 'is-selected' : ''}`}
          key={status}
          type="button"
          aria-label={`Show ${status.replace('_', ' ')} tickets`}
          aria-pressed={statusFilter === status}
          onClick={() => onStatusSelect(status)}
        >
          <span className={`summary-icon icon-${status}`} aria-hidden="true">
            {status === 'open' ? '◷' : status === 'in_progress' ? '↗' : '✓'}
          </span>
          <div>
            <p>{status.replace('_', ' ')}</p>
            <strong>{countFor(summary.byStatus, status)}</strong>
          </div>
          <span className={`status-pip pip-${status}`} />
        </button>
      ))}
      <article className="summary-card priority-summary">
        <div className="priority-title">
          <p>Priority mix</p>
          <strong>{priorityTotal}</strong>
        </div>
        <div className="priority-bars" aria-label="Ticket counts by priority">
          {ticketPriorities.map((priority) => (
            <span
              key={priority}
              className={`priority-segment segment-${priority}`}
              style={{ flexGrow: countFor(summary.byPriority, priority) }}
              title={`${priority}: ${countFor(summary.byPriority, priority)}`}
            />
          ))}
        </div>
        <div className="priority-legend">
          {ticketPriorities.map((priority) => (
            <span key={priority}>
              <i className={`legend-dot segment-${priority}`} />
              {priority}
            </span>
          ))}
        </div>
      </article>
    </section>
  )
}

export default TicketSummary
