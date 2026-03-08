import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, type AdminUserSummary } from '../api/client'
import { useTranslations } from '../hooks/useTranslations'

const ADMIN_KEY_STORAGE = 'coincanvas_admin_key'

export default function Admin() {
  const { t } = useTranslations()
  const navigate = useNavigate()
  const [key, setKey] = useState('')
  const [storedKey, setStoredKey] = useState<string | null>(null)
  const [users, setUsers] = useState<AdminUserSummary[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const saved = sessionStorage.getItem(ADMIN_KEY_STORAGE)
    if (saved) setStoredKey(saved)
  }, [])

  useEffect(() => {
    if (!storedKey) return
    setLoading(true)
    setError(null)
    api.admin
      .users(storedKey)
      .then((data) => {
        setUsers(data)
      })
      .catch((e) => {
        setError(e instanceof Error ? e.message : t('admin_invalid_key'))
        setUsers(null)
        sessionStorage.removeItem(ADMIN_KEY_STORAGE)
        setStoredKey(null)
      })
      .finally(() => setLoading(false))
  }, [storedKey, t])

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    const trimmed = key.trim()
    if (!trimmed) return
    sessionStorage.setItem(ADMIN_KEY_STORAGE, trimmed)
    setStoredKey(trimmed)
    setKey('')
  }

  const handleClearKey = () => {
    sessionStorage.removeItem(ADMIN_KEY_STORAGE)
    setStoredKey(null)
    setUsers(null)
    setError(null)
  }

  return (
    <div className="main" style={{ paddingTop: '2rem' }}>
      <h1 className="page-title">{t('admin_title')}</h1>

      {!storedKey ? (
        <div className="card">
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="admin-key">{t('admin_key_placeholder')}</label>
              <input
                id="admin-key"
                type="password"
                value={key}
                onChange={(e) => setKey(e.target.value)}
                placeholder={t('admin_key_placeholder')}
                autoComplete="off"
              />
            </div>
            <button type="submit" className="btn btn-primary">
              {t('admin_submit')}
            </button>
          </form>
          {error && <p className="error-msg">{error}</p>}
        </div>
      ) : (
        <>
          <div style={{ marginBottom: '1rem' }}>
            <button type="button" className="btn btn-secondary btn-sm" onClick={handleClearKey}>
              {t('admin_clear')}
            </button>
            <button type="button" className="btn btn-secondary btn-sm" style={{ marginLeft: '0.5rem' }} onClick={() => navigate('/')}>
              Back to app
            </button>
          </div>
          {loading && <p className="empty">{t('common_loading')}</p>}
          {error && <p className="error-msg">{error}</p>}
          {users && !loading && (
            <div className="card">
              <h2 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t('admin_users')}</h2>
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>{t('admin_email')}</th>
                      <th>{t('admin_expenses')}</th>
                      <th>{t('admin_categories')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((u) => (
                      <tr key={u.userId}>
                        <td data-label={t('admin_email')}>{u.email}</td>
                        <td data-label={t('admin_expenses')}>{u.expenseCount}</td>
                        <td data-label={t('admin_categories')}>{u.categoryCount}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
}
