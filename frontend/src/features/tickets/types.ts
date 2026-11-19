import type { TicketPriority, TicketStatus } from '../../shared/constants/ticketOptions'

export interface Ticket {
  id: string
  title: string
  description: string | null
  status: TicketStatus
  priority: TicketPriority
  createdAt: string
  updatedAt: string
}

export interface TicketAttachment {
  id: string
  fileName: string
  length: number
  uploadedAt: string
}

export interface TicketPage {
  items: Ticket[]
  totalCount: number
  page: number
  pageSize: number
}

export interface CountByValue {
  value: string
  count: number
}

export interface TicketSummary {
  byStatus: CountByValue[]
  byPriority: CountByValue[]
}

export interface TicketDraft {
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
}

export interface TicketListParameters {
  status?: TicketStatus
  search?: string
  page: number
  pageSize: number
}
