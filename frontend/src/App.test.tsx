import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

describe('App authentication and ticket filters', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  afterEach(() => {
    cleanup()
    vi.unstubAllGlobals()
  })

  it('gates the dashboard behind registration and sends the bearer token to ticket APIs', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = input.toString()
      const jsonResponse = (body: unknown) =>
        new Response(JSON.stringify(body), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })

      if (url.includes('/api/auth/register')) {
        return jsonResponse({
          email: 'new.user@example.test',
          accessToken: 'test-access-token',
          expiresAt: new Date(Date.now() + 60_000).toISOString(),
        })
      }
      if (url.includes('/api/tickets?')) {
        return jsonResponse({ items: [], page: 1, pageSize: 8, totalCount: 0 })
      }
      if (url.includes('/api/tickets/summary')) {
        return jsonResponse({ byStatus: [], byPriority: [] })
      }

      throw new Error(
        `Unexpected request: ${url} ${new Headers(init?.headers).get('Authorization')}`,
      )
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)
    expect(screen.getByRole('heading', { name: 'Create your account' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Sign out' })).not.toBeInTheDocument()

    await user.type(screen.getByLabelText('Work email'), 'new.user@example.test')
    const passwordInput = screen.getByLabelText('Password')
    expect(passwordInput).toHaveAttribute('type', 'password')
    await user.click(screen.getByRole('button', { name: 'Show password' }))
    expect(passwordInput).toHaveAttribute('type', 'text')
    await user.click(screen.getByRole('button', { name: 'Hide password' }))
    await user.type(passwordInput, 'correct horse battery staple')
    const confirmationInput = screen.getByLabelText('Confirm password')
    await user.click(screen.getByRole('button', { name: 'Show password confirmation' }))
    expect(confirmationInput).toHaveAttribute('type', 'text')
    await user.click(screen.getByRole('button', { name: 'Hide password confirmation' }))
    await user.type(confirmationInput, 'correct horse battery staple')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    await screen.findByRole('button', { name: 'Sign out' })
    await waitFor(() => {
      const ticketRequests = fetchMock.mock.calls.filter(([input]) =>
        input.toString().includes('/api/tickets'),
      )
      expect(ticketRequests).toHaveLength(2)
      for (const [, init] of ticketRequests) {
        expect(new Headers(init?.headers).get('Authorization')).toBe('Bearer test-access-token')
      }
    })

    await user.click(screen.getByRole('button', { name: 'Show open tickets' }))
    await waitFor(() => {
      const ticketRequests = fetchMock.mock.calls
        .map(([input, init]) => ({ url: input.toString(), init }))
        .filter(({ url }) => url.includes('/api/tickets?'))
      expect(ticketRequests).toHaveLength(2)
      expect(ticketRequests[1].url).toContain('status=open')
      expect(screen.getByRole('button', { name: 'Show open tickets' })).toHaveAttribute(
        'aria-pressed',
        'true',
      )
    })
  })
})
