import { useState, useEffect } from 'react'
import { useSettings } from '../contexts/SettingsContext'
import { useTranslations } from '../hooks/useTranslations'

const CURRENCIES = ['USD', 'EUR', 'GBP', 'TRY', 'CHF', 'JPY', 'CAD', 'AUD'] as const

const DATE_FORMATS = [
  { value: 'yyyy-MM-dd', labelKey: 'yyyy-MM-dd' },
  { value: 'dd/MM/yyyy', labelKey: 'dd/MM/yyyy' },
  { value: 'MM/dd/yyyy', labelKey: 'MM/dd/yyyy' },
] as const

export default function Settings() {
  const { t } = useTranslations()
  const { settings, updateSettings } = useSettings()
  const [defaultCurrency, setDefaultCurrency] = useState(settings.defaultCurrency)
  const [dateFormat, setDateFormat] = useState(settings.dateFormat)
  const [theme, setTheme] = useState(settings.theme)
  const [language, setLanguage] = useState(settings.language ?? '')
  const [saving, setSaving] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  const themeLabels = [t('settings_light'), t('settings_dark'), t('settings_system')]
  const THEMES = [
    { value: 'light', label: themeLabels[0] },
    { value: 'dark', label: themeLabels[1] },
    { value: 'system', label: themeLabels[2] },
  ] as const

  const LANGUAGES = [
    { value: '', label: t('settings_default_browser') },
    { value: 'en', label: t('settings_lang_english') },
    { value: 'tr', label: t('settings_lang_turkish') },
  ]

  const dateFormatLabels: Record<string, string> = {
    'yyyy-MM-dd': 'YYYY-MM-DD (e.g. 2026-03-07)',
    'dd/MM/yyyy': 'DD/MM/YYYY (e.g. 07/03/2026)',
    'MM/dd/yyyy': 'MM/DD/YYYY (e.g. 03/07/2026)',
  }

  useEffect(() => {
    setDefaultCurrency(settings.defaultCurrency)
    setDateFormat(settings.dateFormat)
    setTheme(settings.theme)
    setLanguage(settings.language ?? '')
  }, [settings])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setMessage(null)
    try {
      await updateSettings({
        defaultCurrency,
        dateFormat,
        theme,
        language: language || null,
      })
      setMessage(t('settings_saved'))
    } catch (err) {
      setMessage(err instanceof Error ? err.message : t('settings_failed'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page">
      <h1>{t('settings_title')}</h1>
      <p className="muted">{t('settings_description')}</p>

      {message && (
        <div className={message === t('settings_saved') ? 'message-success' : 'error'} role="alert">
          {message}
        </div>
      )}

      <section className="card">
        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <label>
              {t('settings_default_currency')}
              <select
                value={defaultCurrency}
                onChange={(e) => setDefaultCurrency(e.target.value)}
                aria-label={t('settings_default_currency')}
              >
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </label>
            <label>
              {t('settings_date_format')}
              <select
                value={dateFormat}
                onChange={(e) => setDateFormat(e.target.value)}
                aria-label={t('settings_date_format')}
              >
                {DATE_FORMATS.map((f) => (
                  <option key={f.value} value={f.value}>{dateFormatLabels[f.value] ?? f.value}</option>
                ))}
              </select>
            </label>
          </div>
          <div className="form-row">
            <label>
              {t('settings_theme')}
              <select
                value={theme}
                onChange={(e) => setTheme(e.target.value as 'light' | 'dark' | 'system')}
                aria-label={t('settings_theme')}
              >
                {THEMES.map((th) => (
                  <option key={th.value} value={th.value}>{th.label}</option>
                ))}
              </select>
            </label>
            <label>
              {t('settings_language')}
              <select
                value={language}
                onChange={(e) => setLanguage(e.target.value)}
                aria-label={t('settings_language')}
              >
                {LANGUAGES.map((l) => (
                  <option key={l.value || 'default'} value={l.value}>{l.label}</option>
                ))}
              </select>
            </label>
          </div>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? t('settings_saving') : t('settings_save')}
          </button>
        </form>
      </section>
    </div>
  )
}
