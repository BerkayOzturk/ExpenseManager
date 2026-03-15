import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { GoogleLogin } from '@react-oauth/google'
import { getToken } from '../auth/storage'
import { useAuth } from '../contexts/AuthContext'
import { useAuthConfig } from '../contexts/AuthConfigContext'
import { useTranslations } from '../hooks/useTranslations'

export default function Register() {
  const { t } = useTranslations()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { register, loginWithGoogle } = useAuth()
  const { googleClientId } = useAuthConfig()
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
        setError(err instanceof Error ? err.message : t('register_failed'))
      } finally {
        setLoading(false)
      }
    },
    [loginWithGoogle, navigate, t]
  )

  const showSocial = !!googleClientId

  return (
    <div className="card" style={{ maxWidth: 360, margin: '2rem auto' }}>
      <p className="register-intro" style={{ marginBottom: '1rem', fontSize: '0.95rem', color: '#475569', lineHeight: 1.4 }}>
        {t('login_intro')}
      </p>
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

      {showSocial && (
        <>
          <p className="register-or" style={{ margin: '1rem 0', textAlign: 'center', fontSize: '0.875rem', color: '#64748b' }}>
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
        {t('register_have_account')} <Link to="/login">{t('register_log_in')}</Link>
      </p>
    </div>
  )
}
