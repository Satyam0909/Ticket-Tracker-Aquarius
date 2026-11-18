import type { AuthSession } from '../auth/authSession'

interface ProblemDetails {
  title?: string
  detail?: string
  code?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  public readonly status: number
  public readonly code?: string

  constructor(message: string, status: number, code?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''
let refreshPromise: Promise<AuthSession> | null = null

async function toApiError(response: Response): Promise<ApiError> {
  const problem = (await response.json().catch(() => null)) as ProblemDetails | null
  const validationMessage = Object.values(problem?.errors ?? {}).flat()[0]
  return new ApiError(
    problem?.detail ?? validationMessage ?? problem?.title ?? `Request failed (${response.status})`,
    response.status,
    problem?.code,
  )
}

export function refreshSession(): Promise<AuthSession> {
  if (!refreshPromise) {
    const sendRefreshRequest = () =>
      fetch(`${apiBaseUrl}/api/auth/refresh`, {
        method: 'POST',
        headers: { Accept: 'application/json, application/problem+json' },
        credentials: 'include',
      })

    refreshPromise = (async () => {
      let response = await sendRefreshRequest()
      if (response.status === 401) {
        // A different tab may have rotated the shared refresh cookie in the meantime.
        await new Promise((resolve) => window.setTimeout(resolve, 200))
        response = await sendRefreshRequest()
      }

      if (!response.ok) {
        const error = await toApiError(response)
        if (response.status === 401) window.dispatchEvent(new Event('auth:expired'))
        throw error
      }
      return (await response.json()) as AuthSession
    })().finally(() => {
      refreshPromise = null
    })
  }
  return refreshPromise
}

export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  headers.set('Accept', 'application/json, application/problem+json')
  if (init?.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const requestOptions: RequestInit = { ...init, headers, credentials: 'include' }
  let response = await fetch(`${apiBaseUrl}${path}`, requestOptions)
  if (response.status === 401 && !path.startsWith('/api/auth/')) {
    try {
      const session = await refreshSession()
      window.dispatchEvent(new CustomEvent<AuthSession>('auth:renewed', { detail: session }))
      response = await fetch(`${apiBaseUrl}${path}`, requestOptions)
      if (response.status === 401) window.dispatchEvent(new Event('auth:expired'))
    } catch (cause: unknown) {
      if (!(cause instanceof ApiError) || cause.status !== 401) throw cause
    }
  }
  if (!response.ok) {
    throw await toApiError(response)
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}
