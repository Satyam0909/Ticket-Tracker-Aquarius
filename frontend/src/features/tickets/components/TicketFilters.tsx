import { ticketStatuses, type TicketStatus } from '../../../shared/constants/ticketOptions'

interface TicketFiltersProps {
  search: string
  status: TicketStatus | ''
  onSearchChange: (value: string) => void
  onStatusChange: (value: TicketStatus | '') => void
}

function TicketFilters({ search, status, onSearchChange, onStatusChange }: TicketFiltersProps) {
  return (
    <div className="toolbar">
      <label className="search-field">
        <span aria-hidden="true">⌕</span>
        <input
          aria-label="Search ticket titles"
          placeholder="Search by title..."
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
        />
      </label>
      <label className="filter-field">
        <span className="visually-hidden">Filter by status</span>
        <select
          value={status}
          onChange={(event) => onStatusChange(event.target.value as TicketStatus | '')}
        >
          <option value="">All statuses</option>
          {ticketStatuses.map((value) => (
            <option key={value} value={value}>
              {value.replace('_', ' ')}
            </option>
          ))}
        </select>
      </label>
    </div>
  )
}

export default TicketFilters
