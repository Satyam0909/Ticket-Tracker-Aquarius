import { useEffect, useState, type ReactNode } from 'react'
import { ApiError } from '../api/httpClient'
import { authApi } from './authApi'
import { AuthContext } from './auth-context'
import type { AuthSession } from './authSession'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(null)
  const [isInitializing, setIsInitializing] = useState(true)
  const [authError, setAuthError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    function handleAuthExpired() {
      setSession(null)
      setAuthError(null)
    }
    function handleAuthRenewed(event: Event) {
      setSession((event as CustomEvent<AuthSession>).detail)
      setAuthError(null)
    }

    window.addEventListener('auth:expired', handleAuthExpired)
    window.addEventListener('auth:renewed', handleAuthRenewed)
    authApi
      .refresh()
      .then((newSession) => {
        if (active) {
          setAuthError(null)
          setSession(newSession)
        }
      })
      .catch((cause: unknown) => {
        if (!(cause instanceof ApiError) || cause.status !== 401) {
          setAuthError('Unable to verify your session. Check your connection and try again.')
        }
      })
      .finally(() => {
        if (active) setIsInitializing(false)
      })

    return () => {
      active = false
      window.removeEventListener('auth:expired', handleAuthExpired)
      window.removeEventListener('auth:renewed', handleAuthRenewed)
    }
  }, [])

  useEffect(() => {
    if (!session) return

    const refreshAt = Date.parse(session.expiresAt) - Date.now() - 60_000
    const timeout = window.setTimeout(
      () => {
        authApi
          .refresh()
          .then((newSession) => {
            setAuthError(null)
            setSession(newSession)
          })
          .catch((cause: unknown) => {
            if (cause instanceof ApiError && cause.status === 401)
              window.dispatchEvent(new Event('auth:expired'))
            else
              setAuthError('Unable to refresh your session. Check your connection and try again.')
          })
      },
      Math.max(1000, refreshAt),
    )

    return () => window.clearTimeout(timeout)
  }, [session])

  async function authenticate(email: string, password: string, register: boolean) {
    const newSession = register
      ? await authApi.register({ email, password })
      : await authApi.login({ email, password })
    setAuthError(null)
    setSession(newSession)
  }

  async function signOut() {
    await authApi.logout()
    setSession(null)
  }

  return (
    <AuthContext.Provider value={{ session, isInitializing, authError, authenticate, signOut }}>
      {children}
    </AuthContext.Provider>
  )
}
