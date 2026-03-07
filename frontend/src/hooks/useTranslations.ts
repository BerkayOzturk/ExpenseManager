import { useMemo } from 'react'
import { useSettings } from '../contexts/SettingsContext'
import { getT, type Locale } from '../translations'

function getLocale(language: string | null): Locale {
  if (language === 'tr') return 'tr'
  return 'en'
}

export function useTranslations(): { t: (key: string) => string; locale: Locale } {
  const { settings } = useSettings()
  const locale = getLocale(settings.language)
  const t = useMemo(() => getT(settings.language), [settings.language])
  return { t, locale }
}
