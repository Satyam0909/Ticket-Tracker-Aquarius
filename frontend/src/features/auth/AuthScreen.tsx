import { useState, type FormEvent } from 'react'
import { ApiError } from '../../shared/api/httpClient'
import { useAuth } from '../../shared/auth/AuthContext'
import './AuthScreen.css'

function AuthScreen() {
  const { authenticate } = useAuth()
  const [isRegistering, setIsRegistering] = useState(true)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmation, setConfirmation] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    if (isRegistering && password !== confirmation) {
      setError('Passwords do not match.')
      return
    }

    setIsSubmitting(true)
    try {
      await authenticate(email, password, isRegistering)
    } catch (cause: unknown) {
      setError(cause instanceof ApiError ? cause.message : 'Unable to authenticate.')
    } finally {
      setIsSubmitting(false)
    }
  }

  function toggleMode() {
    setIsRegistering((value) => !value)
    setError('')
    setPassword('')
    setConfirmation('')
  }

  return (
    <main className="auth-page">
      <section className="auth-card" aria-labelledby="auth-title">
        <a className="brand auth-brand" href="/" aria-label="Resolve home">
          <span className="brand-mark">R</span>
          <span>
            resolve<span className="brand-dot">.</span>
          </span>
        </a>
        <p className="eyebrow">SUPPORT WORKSPACE</p>
        <h1 id="auth-title">{isRegistering ? 'Create your account' : 'Welcome back'}</h1>
        <p className="auth-description">
          {isRegistering
            ? 'Register to access the support ticket workspace.'
            : 'Sign in to continue to your support workspace.'}
        </p>

        <form className="auth-form" onSubmit={submit}>
          <label className="form-label" htmlFor="auth-email">
            Email
          </label>
          <input
            id="auth-email"
            className="form-control auth-input"
            type="email"
            autoComplete="email"
            maxLength={254}
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />

          <label className="form-label" htmlFor="auth-password">
            Password
          </label>
          <input
            id="auth-password"
            className="form-control auth-input"
            type="password"
            autoComplete={isRegistering ? 'new-password' : 'current-password'}
            minLength={isRegistering ? 12 : 1}
            maxLength={128}
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />

          {isRegistering && (
            <>
              <label className="form-label" htmlFor="auth-confirmation">
                Confirm password
              </label>
              <input
                id="auth-confirmation"
                className="form-control auth-input"
                type="password"
                autoComplete="new-password"
                minLength={12}
                maxLength={128}
                required
                value={confirmation}
                onChange={(event) => setConfirmation(event.target.value)}
              />
              <p className="auth-note">All registered accounts can manage every ticket.</p>
            </>
          )}

          {error && (
            <p className="alert error-alert auth-error" role="alert">
              {error}
            </p>
          )}

          <button className="submit-button auth-submit" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Please wait…' : isRegistering ? 'Create account' : 'Sign in'}
          </button>
        </form>

        <p className="auth-switch">
          {isRegistering ? 'Already have an account?' : 'New to Resolve?'}{' '}
          <button type="button" onClick={toggleMode}>
            {isRegistering ? 'Sign in' : 'Create an account'}
          </button>
        </p>
      </section>
    </main>
  )
}

export default AuthScreen
