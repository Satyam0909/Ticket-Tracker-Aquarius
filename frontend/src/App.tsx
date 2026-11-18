import TicketDashboard from './features/tickets/pages/TicketDashboard'
import AuthScreen from './features/auth/AuthScreen'
import { AuthProvider } from './shared/auth/AuthContext'
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
  const { session, signOut } = useAuth()
  if (!session) return <AuthScreen />
  return <TicketDashboard onLogout={signOut} email={session.email} />
}

export default App
