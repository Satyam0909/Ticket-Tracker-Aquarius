import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { authApi } from './authApi'
import {
  clearAuthSession,
  readAuthSession,
  saveAuthSession,
  type AuthSession,
} from './authSession'

interface AuthContextValue {
  session: AuthSession | null
  authenticate(email: string, password: string, register: boolean): Promise<void>
  signOut(): void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(() => readAuthSession())

  useEffect(() => {
    function handleAuthExpired() {
      setSession(null)
    }

    window.addEventListener('auth:expired', handleAuthExpired)
    return () => window.removeEventListener('auth:expired', handleAuthExpired)
  }, [])

  async function authenticate(email: string, password: string, register: boolean) {
    const newSession = register
      ? await authApi.register({ email, password })
      : await authApi.login({ email, password })
    saveAuthSession(newSession)
    setSession(newSession)
  }

  function signOut() {
    clearAuthSession()
    setSession(null)
  }

  return (
    <AuthContext.Provider value={{ session, authenticate, signOut }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider.')
  return context
}
