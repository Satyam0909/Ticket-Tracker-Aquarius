import { useEffect, useState, type ReactNode } from 'react'
import { authApi } from './authApi'
import { AuthContext } from './auth-context'
import { clearAuthSession, readAuthSession, saveAuthSession, type AuthSession } from './authSession'

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
