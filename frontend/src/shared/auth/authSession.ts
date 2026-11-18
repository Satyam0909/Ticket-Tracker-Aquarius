const sessionStorageKey = 'resolve-auth-session'

export interface AuthSession {
  email: string
  accessToken: string
  expiresAt: string
}

export function readAuthSession(): AuthSession | null {
  const stored = sessionStorage.getItem(sessionStorageKey)
  if (!stored) return null

  let session: unknown
  try {
    session = JSON.parse(stored)
  } catch {
    clearAuthSession()
    return null
  }

  if (
    typeof session !== 'object' ||
    session === null ||
    !('email' in session) ||
    typeof session.email !== 'string' ||
    !('accessToken' in session) ||
    typeof session.accessToken !== 'string' ||
    !('expiresAt' in session) ||
    typeof session.expiresAt !== 'string' ||
    !Number.isFinite(Date.parse(session.expiresAt)) ||
    Date.parse(session.expiresAt) <= Date.now()
  ) {
    clearAuthSession()
    return null
  }

  return session as AuthSession
}

export function getAccessToken(): string | null {
  return readAuthSession()?.accessToken ?? null
}

export function saveAuthSession(session: AuthSession): void {
  sessionStorage.setItem(sessionStorageKey, JSON.stringify(session))
}

export function clearAuthSession(): void {
  sessionStorage.removeItem(sessionStorageKey)
}
