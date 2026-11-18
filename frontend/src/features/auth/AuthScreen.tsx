import { useState, type FormEvent } from 'react'
import { ApiError } from '../../shared/api/httpClient'
import { useAuth } from '../../shared/auth/useAuth'
import './AuthScreen.css'

function AuthScreen() {
  const { authenticate } = useAuth()
  const [isRegistering, setIsRegistering] = useState(true)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmation, setConfirmation] = useState('')
  const [isPasswordVisible, setIsPasswordVisible] = useState(false)
  const [isConfirmationVisible, setIsConfirmationVisible] = useState(false)
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
    setIsPasswordVisible(false)
    setIsConfirmationVisible(false)
  }

  return (
    <main className="auth-page">
      <div className="auth-layout">
        <aside className="auth-story">
          <a className="brand auth-brand" href="/" aria-label="Ticket Tracker home">
            <span className="brand-mark" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none">
                <path
                  d="M4.5 7.5h15v3a2 2 0 0 0 0 4v3h-15v-3a2 2 0 0 0 0-4v-3Z"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinejoin="round"
                />
                <path
                  d="M12 8.5v2m0 3v2"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeDasharray="1 1"
                />
              </svg>
            </span>
            <span>Ticket Tracker</span>
          </a>
          <div className="auth-story-copy">
            <p className="auth-story-kicker">SUPPORT, IN SYNC</p>
            <h1>
              Every request.
              <br />
              <span>Moving forward.</span>
            </h1>
            <p className="auth-story-description">
              A clearer view of your team’s work, from the first report to the final resolution.
            </p>
            <div className="auth-feature-list">
              <p>
                <span aria-hidden="true">✓</span> Keep every ticket organized
              </p>
              <p>
                <span aria-hidden="true">✓</span> Track priorities at a glance
              </p>
              <p>
                <span aria-hidden="true">✓</span> Move work ahead together
              </p>
            </div>
          </div>
          <p className="auth-story-footer">ONE WORKSPACE. EVERY FOLLOW-THROUGH.</p>
          <span className="auth-decoration auth-decoration-one" aria-hidden="true" />
          <span className="auth-decoration auth-decoration-two" aria-hidden="true" />
        </aside>

        <section className="auth-card" aria-labelledby="auth-title">
          <div className="auth-heading">
            <p className="auth-kicker">{isRegistering ? 'GET STARTED' : 'WELCOME BACK'}</p>
            <h2 id="auth-title">{isRegistering ? 'Create your account' : 'Sign in to continue'}</h2>
            <p className="auth-description">
              {isRegistering
                ? 'Create an account to access your support workspace.'
                : 'Enter your account details to open your workspace.'}
            </p>
          </div>

          <form className="auth-form" onSubmit={submit}>
            <label className="form-label" htmlFor="auth-email">
              Work email
            </label>
            <input
              id="auth-email"
              className="form-control auth-input"
              type="email"
              autoComplete="email"
              maxLength={254}
              placeholder="you@company.com"
              required
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />

            <label className="form-label" htmlFor="auth-password">
              Password
            </label>
            <div className="password-field">
              <input
                id="auth-password"
                className="form-control auth-input"
                type={isPasswordVisible ? 'text' : 'password'}
                autoComplete={isRegistering ? 'new-password' : 'current-password'}
                minLength={isRegistering ? 12 : 1}
                maxLength={128}
                placeholder={isRegistering ? 'At least 12 characters' : 'Enter your password'}
                required
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
              <PasswordVisibilityToggle
                visible={isPasswordVisible}
                onToggle={() => setIsPasswordVisible((value) => !value)}
                label="password"
              />
            </div>

            {isRegistering && (
              <>
                <label className="form-label" htmlFor="auth-confirmation">
                  Confirm password
                </label>
                <div className="password-field">
                  <input
                    id="auth-confirmation"
                    className="form-control auth-input"
                    type={isConfirmationVisible ? 'text' : 'password'}
                    autoComplete="new-password"
                    minLength={12}
                    maxLength={128}
                    placeholder="Re-enter your password"
                    required
                    value={confirmation}
                    onChange={(event) => setConfirmation(event.target.value)}
                  />
                  <PasswordVisibilityToggle
                    visible={isConfirmationVisible}
                    onToggle={() => setIsConfirmationVisible((value) => !value)}
                    label="password confirmation"
                  />
                </div>
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
              {!isSubmitting && <span aria-hidden="true">→</span>}
            </button>
          </form>

          <p className="auth-switch">
            {isRegistering ? 'Already have an account?' : 'New to Ticket Tracker?'}{' '}
            <button type="button" onClick={toggleMode}>
              {isRegistering ? 'Sign in' : 'Create an account'}
            </button>
          </p>
          <p className="auth-security-note">
            <span aria-hidden="true">◆</span> Your account is protected with secure sign-in.
          </p>
        </section>
      </div>
    </main>
  )
}

function PasswordVisibilityToggle({
  visible,
  onToggle,
  label,
}: {
  visible: boolean
  onToggle(): void
  label: string
}) {
  return (
    <button
      className="password-toggle"
      type="button"
      aria-label={`${visible ? 'Hide' : 'Show'} ${label}`}
      aria-pressed={visible}
      onClick={onToggle}
    >
      <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
        {visible ? (
          <>
            <path
              d="M3 3l18 18M10.6 10.6a2 2 0 0 0 2.8 2.8"
              stroke="currentColor"
              strokeWidth="1.7"
              strokeLinecap="round"
            />
            <path
              d="M9.9 5.2A10.7 10.7 0 0 1 12 5c5.2 0 8.5 4.2 9.5 6-.4.8-1.3 2-2.6 3.2M6.2 6.2C4.1 7.5 2.8 9.4 2.5 11c.5.9 1.8 2.7 4 4"
              stroke="currentColor"
              strokeWidth="1.7"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </>
        ) : (
          <path
            d="M2.5 12s3.2-6 9.5-6 9.5 6 9.5 6-3.2 6-9.5 6-9.5-6-9.5-6Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
        )}
        {!visible && <circle cx="12" cy="12" r="2.5" stroke="currentColor" strokeWidth="1.7" />}
      </svg>
    </button>
  )
}

export default AuthScreen
