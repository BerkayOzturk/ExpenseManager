import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { forgotPassword, resetPassword } from '../api/auth'
import { useTranslations } from '../hooks/useTranslations'

export default function ForgotPassword() {
  const { t } = useTranslations()
  const navigate = useNavigate()
  const [step, setStep] = useState<1 | 2>(1)
  const [email, setEmail] = useState('')
  const [code, setCode] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  async function handleSendCode(e: React.FormEvent) {
    e.preventDefault()
    const trimmed = email.trim()
    if (!trimmed) return
    setError(null)
    setMessage(null)
    setLoading(true)
    try {
      await forgotPassword(trimmed)
      setMessage(t('forgot_success'))
      setStep(2)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Request failed')
    } finally {
      setLoading(false)
    }
  }

  async function handleReset(e: React.FormEvent) {
    e.preventDefault()
    const trimmedEmail = email.trim()
    const trimmedCode = code.trim()
    if (!trimmedEmail || !trimmedCode || newPassword.length < 6) {
      setError(t('register_password_min'))
      return
    }
    setError(null)
    setMessage(null)
    setLoading(true)
    try {
      await resetPassword(trimmedEmail, trimmedCode, newPassword)
      setMessage(t('forgot_reset_success'))
      setTimeout(() => navigate('/login', { replace: true }), 2000)
    } catch (err) {
      setError(err instanceof Error ? err.message : t('forgot_invalid_code'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="card" style={{ maxWidth: 400, margin: '2rem auto' }}>
      <h1 className="page-title" style={{ marginBottom: '1rem' }}>{t('forgot_title')}</h1>

      {step === 1 && (
        <form onSubmit={handleSendCode}>
          <p style={{ marginBottom: '1rem', fontSize: '0.875rem', color: '#64748b' }}>
            {t('forgot_step1_instruction')}
          </p>
          <div className="form-group" style={{ marginBottom: '1rem' }}>
            <label htmlFor="forgot-email">{t('login_email')}</label>
            <input
              id="forgot-email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%', marginTop: '0.5rem' }}>
            {loading ? t('forgot_sending') : t('forgot_send_code')}
          </button>
        </form>
      )}

      {step === 2 && (
        <form onSubmit={handleReset}>
          <p style={{ marginBottom: '1rem', fontSize: '0.875rem', color: '#64748b' }}>
            {t('forgot_step2_instruction')}
          </p>
          <div className="form-group" style={{ marginBottom: '1rem' }}>
            <label htmlFor="forgot-email-readonly">{t('login_email')}</label>
            <input
              id="forgot-email-readonly"
              type="email"
              value={email}
              readOnly
              style={{ backgroundColor: 'var(--input-bg-readonly, #f1f5f9)', cursor: 'not-allowed' }}
            />
          </div>
          <div className="form-group" style={{ marginBottom: '1rem' }}>
            <label htmlFor="forgot-code">{t('forgot_code')}</label>
            <input
              id="forgot-code"
              type="text"
              inputMode="numeric"
              autoComplete="one-time-code"
              maxLength={6}
              placeholder="000000"
              value={code}
              onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))}
              required
            />
          </div>
          <div className="form-group" style={{ marginBottom: '1rem' }}>
            <label htmlFor="forgot-new-password">{t('forgot_new_password')}</label>
            <input
              id="forgot-new-password"
              type="password"
              autoComplete="new-password"
              minLength={6}
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          {message && <p className="success-msg" style={{ color: 'var(--success, #059669)' }}>{message}</p>}
          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%', marginTop: '0.5rem' }}>
            {loading ? t('forgot_resetting') : t('forgot_reset')}
          </button>
          <button
            type="button"
            className="btn btn-secondary"
            style={{ width: '100%', marginTop: '0.5rem' }}
            onClick={() => { setStep(1); setCode(''); setNewPassword(''); setError(null); setMessage(null); }}
          >
            {t('forgot_different_email')}
          </button>
        </form>
      )}

      {message && step === 1 && <p style={{ marginTop: '1rem', fontSize: '0.875rem', color: 'var(--success, #059669)' }}>{message}</p>}

      <p style={{ marginTop: '1rem', fontSize: '0.875rem', color: '#64748b' }}>
        <Link to="/login">{t('register_log_in')}</Link>
      </p>
    </div>
  )
}
