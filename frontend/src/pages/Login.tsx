import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getToken } from '../auth/storage'
import { useAuth } from '../contexts/AuthContext'
import { useTranslations } from '../hooks/useTranslations'

export default function Login() {
  const { t } = useTranslations()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (getToken()) navigate('/', { replace: true })
  }, [navigate])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await login(email, password)
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : t('login_failed'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="card" style={{ maxWidth: 360, margin: '2rem auto' }}>
      <h1 className="page-title" style={{ marginBottom: '1rem' }}>{t('login_title')}</h1>
      <form onSubmit={handleSubmit}>
        <div className="form-group" style={{ marginBottom: '1rem' }}>
          <label htmlFor="email">{t('login_email')}</label>
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
          <label htmlFor="password">{t('login_password')}</label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        {error && <p className="error-msg">{error}</p>}
        <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%', marginTop: '0.5rem' }}>
          {loading ? t('login_signing_in') : t('login_submit')}
        </button>
      </form>
      <p style={{ marginTop: '1rem', fontSize: '0.875rem', color: '#64748b' }}>
        {t('login_no_account')} <Link to="/register">{t('login_register')}</Link>
      </p>
    </div>
  )
}
