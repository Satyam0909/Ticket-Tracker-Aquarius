import { createContext } from 'react'
import type { AuthSession } from './authSession'

export interface AuthContextValue {
  session: AuthSession | null
  isInitializing: boolean
  authError: string | null
  authenticate(email: string, password: string, register: boolean): Promise<void>
  signOut(): Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
