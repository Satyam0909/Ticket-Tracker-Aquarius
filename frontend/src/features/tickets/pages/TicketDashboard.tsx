import { useEffect, useState } from 'react'
import { ticketsApi } from '../api/ticketsApi'
import TicketForm from '../components/TicketForm'
import TicketList from '../components/TicketList'
import TicketSummary from '../components/TicketSummary'
import type { Ticket, TicketDraft, TicketPage, TicketSummary as TicketSummaryModel } from '../types'
import type { TicketStatus } from '../../../shared/constants/ticketOptions'
import { ApiError } from '../../../shared/api/httpClient'
import { formatToday } from '../../../shared/utils/formatDate'

const pageSize = 8
const emptySummary: TicketSummaryModel = { byStatus: [], byPriority: [] }

interface TicketDashboardProps {
  email: string
  authError: string | null
  onLogout(): Promise<void>
}

function TicketDashboard({ email, authError, onLogout }: TicketDashboardProps) {
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [summary, setSummary] = useState<TicketSummaryModel>(emptySummary)
  const [statusFilter, setStatusFilter] = useState<TicketStatus | ''>('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [logoutError, setLogoutError] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    Promise.all([
      ticketsApi.list(
        { status: statusFilter || undefined, search, page, pageSize },
        controller.signal,
      ),
      ticketsApi.getSummary(controller.signal),
    ])
      .then(([result, counts]: [TicketPage, TicketSummaryModel]) => {
        setTickets(result.items)
        setTotalCount(result.totalCount)
        setSummary(counts)
      })
      .catch((cause: unknown) => {
        if (cause instanceof Error && cause.name === 'AbortError') return
        setError(cause instanceof ApiError ? cause.message : 'Unable to load tickets.')
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [statusFilter, search, page, refreshVersion])

  async function createTicket(draft: TicketDraft, files: File[]) {
    const ticket = await ticketsApi.create(draft)
    if (files.length > 0) {
      try {
        await ticketsApi.uploadAttachments(ticket.id, files)
      } catch (cause: unknown) {
        setError(
          `Ticket created, but attachments could not be uploaded: ${
            cause instanceof Error ? cause.message : 'try again from the ticket attachments.'
          }`,
        )
      }
    }
    setIsLoading(true)
    setPage(1)
    setRefreshVersion((value) => value + 1)
  }

  async function updateTicketStatus(ticket: Ticket, status: TicketStatus) {
    setError('')
    try {
      await ticketsApi.update(ticket, status)
      setIsLoading(true)
      setRefreshVersion((value) => value + 1)
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to update ticket.')
    }
  }

  async function deleteTicket(id: string) {
    setError('')
    try {
      await ticketsApi.delete(id)
      setIsLoading(true)
      setRefreshVersion((value) => value + 1)
    } catch (cause: unknown) {
      setError(cause instanceof Error ? cause.message : 'Unable to delete ticket.')
    }
  }

  function changeSearch(value: string) {
    setIsLoading(true)
    setError('')
    setSearch(value)
    setPage(1)
  }

  function changeStatusFilter(value: TicketStatus | '') {
    setIsLoading(true)
    setError('')
    setStatusFilter(value)
    setPage(1)
  }

  function changePage(value: number) {
    setIsLoading(true)
    setPage(value)
  }

  async function handleLogout() {
    setLogoutError('')
    try {
      await onLogout()
    } catch (cause: unknown) {
      setLogoutError(cause instanceof Error ? cause.message : 'Unable to sign out.')
    }
  }

  return (
    <main className="app-shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Ticket Tracker home">
          <span className="brand-mark" aria-hidden="true">
            <svg viewBox="0 0 24 24" fill="none">
              <path
                d="M4.5 7.5h15v3a2 2 0 0 0 0 4v3h-15v-3a2 2 0 0 0 0-4v-3Z"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinejoin="round"
              />
              <path
                d="M12 8.5v2m0 3v2"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeDasharray="1 1"
              />
            </svg>
          </span>
          <span>Ticket Tracker</span>
        </a>
        <div className="account-actions">
          <span className="team-label">
            <span className="online-dot" /> {email}
          </span>
          <button className="logout-button" type="button" onClick={() => void handleLogout()}>
            Sign out
          </button>
        </div>
      </header>

      {(authError || logoutError) && (
        <p className="alert error-alert" role="alert">
          {logoutError || authError}
        </p>
      )}

      <section className="welcome">
        <div>
          <p className="eyebrow">TICKET TRACKER / OVERVIEW</p>
          <h1>
            Support work, <span>under control.</span>
          </h1>
          <p className="subheading">Prioritize clearly. Resolve confidently.</p>
        </div>
        <div className="date-chip">{formatToday()}</div>
      </section>

      <TicketSummary
        summary={summary}
        statusFilter={statusFilter}
        onStatusSelect={changeStatusFilter}
      />

      <div className="workspace-grid">
        <TicketList
          tickets={tickets}
          totalCount={totalCount}
          page={page}
          pageSize={pageSize}
          isLoading={isLoading}
          error={error}
          search={search}
          status={statusFilter}
          onSearchChange={changeSearch}
          onStatusFilterChange={changeStatusFilter}
          onStatusChange={updateTicketStatus}
          onDelete={deleteTicket}
          onPageChange={changePage}
        />
        <TicketForm onCreate={createTicket} />
      </div>

      <footer className="page-footer">
        <span>TICKET TRACKER</span>
        <span>Every request has a clear next step.</span>
      </footer>
    </main>
  )
}

export default TicketDashboard
