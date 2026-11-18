import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { TicketDraft } from '../types'
import TicketForm from './TicketForm'

describe('TicketForm', () => {
  it('blocks empty titles and submits a valid ticket', async () => {
    const user = userEvent.setup()
    const onCreate = vi.fn(async (draft: TicketDraft) => {
      expect(draft.title).toBe('Cannot sign in')
    })
    render(<TicketForm onCreate={onCreate} />)

    await user.click(screen.getByRole('button', { name: /create ticket/i }))
    expect(screen.getByRole('alert')).toHaveTextContent('Enter a ticket title.')
    expect(onCreate).not.toHaveBeenCalled()

    await user.type(screen.getByLabelText(/ticket title/i), 'Cannot sign in')
    fireEvent.submit(screen.getByRole('button', { name: /create ticket/i }).closest('form')!)
    expect(
      await screen.findByText('Your team will be able to pick this up right away.'),
    ).toBeVisible()
    expect(onCreate).toHaveBeenCalledWith({
      title: 'Cannot sign in',
      description: '',
      status: 'open',
      priority: 'medium',
    })
  })
})
