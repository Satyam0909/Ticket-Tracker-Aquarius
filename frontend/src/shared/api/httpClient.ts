import { clearAuthSession, getAccessToken } from '../auth/authSession'

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

export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  headers.set('Accept', 'application/json, application/problem+json')
  const accessToken = getAccessToken()
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)
  if (init?.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(`${apiBaseUrl}${path}`, { ...init, headers })
  if (response.status === 401) {
    clearAuthSession()
    window.dispatchEvent(new Event('auth:expired'))
  }
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null
    const validationMessage = Object.values(problem?.errors ?? {}).flat()[0]
    throw new ApiError(
      problem?.detail ??
        validationMessage ??
        problem?.title ??
        `Request failed (${response.status})`,
      response.status,
      problem?.code,
    )
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}
