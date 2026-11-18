import { request } from '../../../shared/api/httpClient'
import type { Ticket, TicketDraft, TicketListParameters, TicketPage, TicketSummary } from '../types'

function buildListQuery(parameters: TicketListParameters): string {
  const query = new URLSearchParams({
    page: String(parameters.page),
    pageSize: String(parameters.pageSize),
  })
  if (parameters.status) query.set('status', parameters.status)
  if (parameters.search?.trim()) query.set('search', parameters.search.trim())
  return query.toString()
}

export const ticketsApi = {
  list(parameters: TicketListParameters, signal?: AbortSignal): Promise<TicketPage> {
    return request<TicketPage>(`/api/tickets?${buildListQuery(parameters)}`, { signal })
  },

  getSummary(signal?: AbortSignal): Promise<TicketSummary> {
    return request<TicketSummary>('/api/tickets/summary', { signal })
  },

  create(draft: TicketDraft): Promise<Ticket> {
    return request<Ticket>('/api/tickets', {
      method: 'POST',
      body: JSON.stringify(draft),
    })
  },

  update(ticket: Ticket, status: Ticket['status']): Promise<Ticket> {
    return request<Ticket>(`/api/tickets/${ticket.id}`, {
      method: 'PUT',
      body: JSON.stringify({
        title: ticket.title,
        description: ticket.description,
        status,
        priority: ticket.priority,
      }),
    })
  },

  delete(id: string): Promise<void> {
    return request<void>(`/api/tickets/${id}`, { method: 'DELETE' })
  },
}
