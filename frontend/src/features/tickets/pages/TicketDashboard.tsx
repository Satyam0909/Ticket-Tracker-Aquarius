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
  onLogout(): void
}

function TicketDashboard({ email, onLogout }: TicketDashboardProps) {
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [summary, setSummary] = useState<TicketSummaryModel>(emptySummary)
  const [statusFilter, setStatusFilter] = useState<TicketStatus | ''>('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

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

  async function createTicket(draft: TicketDraft) {
    await ticketsApi.create(draft)
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

  return (
    <main className="app-shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Resolve home">
          <span className="brand-mark">R</span>
          <span>
            resolve<span className="brand-dot">.</span>
          </span>
        </a>
        <div className="account-actions">
          <span className="team-label">
            <span className="online-dot" /> {email}
          </span>
          <button className="logout-button" type="button" onClick={onLogout}>
            Sign out
          </button>
        </div>
      </header>

      <section className="welcome">
        <div>
          <p className="eyebrow">CUSTOMER CARE / OVERVIEW</p>
          <h1>
            Good work starts with <span>good support.</span>
          </h1>
          <p className="subheading">Keep every conversation moving forward.</p>
        </div>
        <div className="date-chip">{formatToday()}</div>
      </section>

      <TicketSummary summary={summary} />

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
        <span>RESOLVE SUPPORT TRACKER</span>
        <span>Made for teams that care.</span>
      </footer>
    </main>
  )
}

export default TicketDashboard
