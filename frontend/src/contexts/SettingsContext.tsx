import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { useAuth } from './AuthContext'
import { api } from '../api/client'
import type { UserSettings } from '../types'

const DEFAULT_SETTINGS: UserSettings = {
  defaultCurrency: 'USD',
  dateFormat: 'yyyy-MM-dd',
  theme: 'system',
  language: null,
}

type ThemeValue = 'light' | 'dark' | 'system'

interface SettingsContextValue {
  settings: UserSettings
  loading: boolean
  updateSettings: (s: Partial<UserSettings>) => Promise<void>
}

const SettingsContext = createContext<SettingsContextValue | null>(null)

function getEffectiveTheme(theme: string): 'light' | 'dark' {
  if (theme === 'light') return 'light'
  if (theme === 'dark') return 'dark'
  if (typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches) return 'dark'
  return 'light'
}

function applyTheme(theme: string) {
  const effective = getEffectiveTheme(theme)
  document.documentElement.setAttribute('data-theme', effective)
}

export function SettingsProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth()
  const [settings, setSettings] = useState<UserSettings>(DEFAULT_SETTINGS)
  const [loading, setLoading] = useState(true)

  const loadSettings = useCallback(async () => {
    if (!isAuthenticated) {
      setSettings(DEFAULT_SETTINGS)
      setLoading(false)
      applyTheme('system')
      return
    }
    setLoading(true)
    try {
      const data = await api.settings.get()
      setSettings({
        defaultCurrency: data.defaultCurrency ?? 'USD',
        dateFormat: data.dateFormat ?? 'yyyy-MM-dd',
        theme: (data.theme as ThemeValue) ?? 'system',
        language: data.language ?? null,
      })
      applyTheme(data.theme ?? 'system')
    } catch {
      setSettings(DEFAULT_SETTINGS)
      applyTheme('system')
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated])

  useEffect(() => {
    loadSettings()
  }, [loadSettings])

  const updateSettings = useCallback(async (next: Partial<UserSettings>) => {
    const payload = {
      defaultCurrency: next.defaultCurrency ?? settings.defaultCurrency,
      dateFormat: next.dateFormat ?? settings.dateFormat,
      theme: next.theme ?? settings.theme,
      language: next.language !== undefined ? next.language : settings.language,
    }
    const data = await api.settings.update(payload)
    setSettings({
      defaultCurrency: data.defaultCurrency,
      dateFormat: data.dateFormat,
      theme: data.theme as ThemeValue,
      language: data.language ?? null,
    })
    applyTheme(data.theme)
  }, [settings])

  return (
    <SettingsContext.Provider value={{ settings, loading, updateSettings }}>
      {children}
    </SettingsContext.Provider>
  )
}

export function useSettings() {
  const ctx = useContext(SettingsContext)
  if (!ctx) throw new Error('useSettings must be used within SettingsProvider')
  return ctx
}
