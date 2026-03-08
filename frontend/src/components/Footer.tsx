import { useTranslations } from '../hooks/useTranslations'

export function Footer() {
  const { t } = useTranslations()
  const year = new Date().getFullYear()
  const copyright = t('footer_copyright').replace('{year}', String(year))

  return (
    <footer className="site-footer">
      <div className="site-footer__inner">
        <span className="site-footer__name">{t('footer_app_name')}</span>
        <span className="site-footer__tagline">{t('footer_tagline')}</span>
        <span className="site-footer__copy">{copyright}</span>
      </div>
    </footer>
  )
}
