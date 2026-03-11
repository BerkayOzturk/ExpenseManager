import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { GoogleLogin } from '@react-oauth/google'
import { getToken } from '../auth/storage'
import { useAuth } from '../contexts/AuthContext'
import { useAuthConfig } from '../contexts/AuthConfigContext'
import { useTranslations } from '../hooks/useTranslations'

export default function Login() {
  const { t } = useTranslations()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { login, loginWithGoogle } = useAuth()
  const { googleClientId } = useAuthConfig()
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

  const handleGoogleSuccess = useCallback(
    async (credentialResponse: { credential?: string }) => {
      const token = credentialResponse.credential
      if (!token) return
      setError(null)
      setLoading(true)
      try {
        await loginWithGoogle(token)
        navigate('/', { replace: true })
      } catch (err) {
        setError(err instanceof Error ? err.message : t('login_failed'))
      } finally {
        setLoading(false)
      }
    },
    [loginWithGoogle, navigate, t]
  )

  const showSocial = !!googleClientId

  return (
    <div className="card" style={{ maxWidth: 360, margin: '2rem auto' }}>
      <p className="login-intro" style={{ marginBottom: '1rem', fontSize: '0.95rem', color: '#475569', lineHeight: 1.4 }}>
        {t('login_intro')}
      </p>
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
        <div className="form-group" style={{ marginBottom: '0.5rem' }}>
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
        <p style={{ marginBottom: '1rem', fontSize: '0.875rem' }}>
          <Link to="/forgot-password">{t('login_forgot_password')}</Link>
        </p>
        {error && <p className="error-msg">{error}</p>}
        <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%', marginTop: '0.5rem' }}>
          {loading ? t('login_signing_in') : t('login_submit')}
        </button>
      </form>

      {showSocial && (
        <>
          <p className="login-or" style={{ margin: '1rem 0', textAlign: 'center', fontSize: '0.875rem', color: '#64748b' }}>
            {t('login_or')}
          </p>
          <div className="social-buttons" style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {googleClientId && (
              <div className="social-btn-wrap" style={{ display: 'flex', justifyContent: 'center' }}>
                <GoogleLogin
                  onSuccess={handleGoogleSuccess}
                  onError={() => setError('Google sign-in failed')}
                  useOneTap={false}
                  theme="outline"
                  size="large"
                  type="standard"
                  text="signin_with"
                  shape="rectangular"
                  width="320"
                />
              </div>
            )}
          </div>
        </>
      )}

      <p style={{ marginTop: '1rem', fontSize: '0.875rem', color: '#64748b' }}>
        {t('login_no_account')} <Link to="/register">{t('login_register')}</Link>
      </p>
    </div>
  )
}
