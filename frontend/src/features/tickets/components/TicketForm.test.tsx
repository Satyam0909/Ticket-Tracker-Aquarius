import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { TicketDraft } from '../types'
import TicketForm from './TicketForm'

afterEach(cleanup)

describe('TicketForm', () => {
  it('blocks empty titles and submits a valid ticket', async () => {
    const user = userEvent.setup({ applyAccept: false })
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
    expect(onCreate).toHaveBeenCalledWith(
      {
        title: 'Cannot sign in',
        description: '',
        status: 'open',
        priority: 'medium',
      },
      [],
    )
  })

  it('accepts supported attachments when creating a ticket', async () => {
    const user = userEvent.setup({ applyAccept: false })
    const onCreate = vi.fn(async () => {})
    render(<TicketForm onCreate={onCreate} />)

    await user.type(screen.getByLabelText(/ticket title/i), 'CAD issue')
    const file = new File(['model'], 'model.STL')
    await user.upload(screen.getByLabelText(/attachments/i), file)
    await user.click(screen.getByRole('button', { name: /create ticket/i }))

    expect(onCreate).toHaveBeenCalledWith(
      {
        title: 'CAD issue',
        description: '',
        status: 'open',
        priority: 'medium',
      },
      [file],
    )
  })

  it('blocks unsupported files and more than three attachments', async () => {
    const user = userEvent.setup({ applyAccept: false })
    const onCreate = vi.fn(async () => {})
    render(<TicketForm onCreate={onCreate} />)
    await user.type(screen.getByLabelText(/ticket title/i), 'Upload issue')

    await user.upload(screen.getByLabelText(/attachments/i), new File(['x'], 'script.exe'))
    expect(screen.getByRole('alert')).toHaveTextContent('file type is not allowed')
    await user.click(screen.getByRole('button', { name: /create ticket/i }))
    expect(onCreate).not.toHaveBeenCalled()

    const validFiles = [
      new File(['1'], 'one.txt'),
      new File(['2'], 'two.txt'),
      new File(['3'], 'three.txt'),
    ]
    await user.upload(screen.getByLabelText(/attachments/i), validFiles)
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    await user.upload(screen.getByLabelText(/attachments/i), new File(['4'], 'four.txt'))
    expect(screen.getByRole('alert')).toHaveTextContent('no more than 3 attachments')
    expect(onCreate).not.toHaveBeenCalled()
  })
})
