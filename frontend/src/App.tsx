import TicketDashboard from './features/tickets/pages/TicketDashboard'
import AuthScreen from './features/auth/AuthScreen'
import { AuthProvider } from './shared/auth/AuthProvider'
import { useAuth } from './shared/auth/useAuth'
import './App.css'

function App() {
  return (
    <AuthProvider>
      <AuthenticatedApp />
    </AuthProvider>
  )
}

function AuthenticatedApp() {
  const { session, signOut, isInitializing, authError } = useAuth()
  if (isInitializing) {
    return (
      <main className="auth-initializing" role="status">
        <span className="spinner" />
        <span>Checking your session…</span>
      </main>
    )
  }
  if (!session) return <AuthScreen initializationError={authError} />
  return <TicketDashboard onLogout={signOut} email={session.email} authError={authError} />
}

export default App
