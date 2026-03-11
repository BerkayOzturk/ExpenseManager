import { Link } from 'react-router-dom'
import { useTranslations } from '../hooks/useTranslations'

export default function Terms() {
  const { t } = useTranslations()

  return (
    <div className="card legal-page">
      <h1 className="page-title">{t('terms_title')}</h1>
      <p className="legal-updated">{t('terms_updated')}</p>

      <section>
        <h2>{t('terms_acceptance_heading')}</h2>
        <p>{t('terms_acceptance_body')}</p>
      </section>

      <section>
        <h2>{t('terms_service_heading')}</h2>
        <p>{t('terms_service_body')}</p>
      </section>

      <section>
        <h2>{t('terms_conduct_heading')}</h2>
        <p>{t('terms_conduct_body')}</p>
      </section>

      <section>
        <h2>{t('terms_liability_heading')}</h2>
        <p>{t('terms_liability_body')}</p>
      </section>

      <section>
        <h2>{t('terms_changes_heading')}</h2>
        <p>{t('terms_changes_body')}</p>
      </section>

      <p style={{ marginTop: '2rem' }}>
        <Link to="/" className="link">{t('common_back')}</Link>
      </p>
    </div>
  )
}
