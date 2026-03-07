import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getToken } from '../auth/storage'
import { useAuth } from '../contexts/AuthContext'
import { useTranslations } from '../hooks/useTranslations'

export default function Register() {
  const { t } = useTranslations()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { register } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (getToken()) navigate('/', { replace: true })
  }, [navigate])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    if (password.length < 6) {
      setError(t('register_password_min'))
      return
    }
    setLoading(true)
    try {
      await register(email, password)
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : t('register_failed'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="card" style={{ maxWidth: 360, margin: '2rem auto' }}>
      <h1 className="page-title" style={{ marginBottom: '1rem' }}>{t('register_title')}</h1>
      <form onSubmit={handleSubmit}>
        <div className="form-group" style={{ marginBottom: '1rem' }}>
          <label htmlFor="email">{t('register_email')}</label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div className="form-group" style={{ marginBottom: '1rem' }}>
          <label htmlFor="password">{t('register_password')}</label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={6}
          />
        </div>
        {error && <p className="error-msg">{error}</p>}
        <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%', marginTop: '0.5rem' }}>
          {loading ? t('register_creating') : t('register_submit')}
        </button>
      </form>
      <p style={{ marginTop: '1rem', fontSize: '0.875rem', color: '#64748b' }}>
        {t('register_have_account')} <Link to="/login">{t('register_log_in')}</Link>
      </p>
    </div>
  )
}
