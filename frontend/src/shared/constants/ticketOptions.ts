export const ticketStatuses = ['open', 'in_progress', 'closed'] as const
export const ticketPriorities = ['low', 'medium', 'high'] as const

export type TicketStatus = (typeof ticketStatuses)[number]
export type TicketPriority = (typeof ticketPriorities)[number]
