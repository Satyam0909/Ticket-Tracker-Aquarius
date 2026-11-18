import { ticketStatuses, type TicketStatus } from '../../../shared/constants/ticketOptions'
import { formatDate } from '../../../shared/utils/formatDate'
import { getPaginationRange } from '../../../shared/utils/pagination'
import TicketFilters from './TicketFilters'
import type { Ticket } from '../types'

interface TicketListProps {
  tickets: Ticket[]
  totalCount: number
  page: number
  pageSize: number
  isLoading: boolean
  error: string
  search: string
  status: TicketStatus | ''
  onSearchChange: (value: string) => void
  onStatusFilterChange: (value: TicketStatus | '') => void
  onStatusChange: (ticket: Ticket, status: TicketStatus) => void
  onDelete: (id: string) => void
  onPageChange: (page: number) => void
}

function TicketList({
  tickets,
  totalCount,
  page,
  pageSize,
  isLoading,
  error,
  search,
  status,
  onSearchChange,
  onStatusFilterChange,
  onStatusChange,
  onDelete,
  onPageChange,
}: TicketListProps) {
  const { first, last, pageCount } = getPaginationRange(page, pageSize, totalCount)

  return (
    <section className="ticket-panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">WORK QUEUE</p>
          <h2>
            Tickets <span className="count-pill">{totalCount}</span>
          </h2>
        </div>
        <span className="updated-label">
          <span className="online-dot" /> Live overview
        </span>
      </div>
      <TicketFilters
        search={search}
        status={status}
        onSearchChange={onSearchChange}
        onStatusChange={onStatusFilterChange}
      />

      {error && (
        <div className="alert error-alert" role="alert">
          {error}
        </div>
      )}
      {isLoading ? (
        <div className="empty-state" role="status">
          <span className="spinner" /> Loading tickets…
        </div>
      ) : tickets.length === 0 ? (
        <div className="empty-state">
          <span className="empty-icon">✳</span>
          <strong>No tickets found</strong>
          <span>Try adjusting your search or create a ticket.</span>
        </div>
      ) : (
        <div className="ticket-list">
          {tickets.map((ticket) => (
            <article className="ticket-row" key={ticket.id}>
              <span
                className={`priority-marker marker-${ticket.priority}`}
                title={`${ticket.priority} priority`}
              />
              <div className="ticket-copy">
                <h3>{ticket.title}</h3>
                <p>{ticket.description || 'No description provided'}</p>
                <span className="ticket-meta">
                  #{ticket.id.slice(0, 8)} · {formatDate(ticket.createdAt)}
                </span>
              </div>
              <div className="ticket-actions">
                <label className={`status-select status-${ticket.status}`}>
                  <span className="visually-hidden">Change status for {ticket.title}</span>
                  <select
                    value={ticket.status}
                    onChange={(event) => onStatusChange(ticket, event.target.value as TicketStatus)}
                  >
                    {ticketStatuses.map((value) => (
                      <option key={value} value={value}>
                        {value.replace('_', ' ')}
                      </option>
                    ))}
                  </select>
                </label>
                <button
                  className="delete-button"
                  type="button"
                  aria-label={`Delete ${ticket.title}`}
                  onClick={() => onDelete(ticket.id)}
                >
                  ×
                </button>
              </div>
            </article>
          ))}
        </div>
      )}

      <footer className="list-footer">
        <span>
          Showing {first}–{last} of {totalCount}
        </span>
        <div className="pagination">
          <button
            type="button"
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1 || isLoading}
          >
            ← Previous
          </button>
          <span>
            {page} / {pageCount}
          </span>
          <button
            type="button"
            onClick={() => onPageChange(page + 1)}
            disabled={page >= pageCount || isLoading}
          >
            Next →
          </button>
        </div>
      </footer>
    </section>
  )
}

export default TicketList
