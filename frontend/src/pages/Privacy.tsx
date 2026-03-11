import { Link } from 'react-router-dom'
import { useTranslations } from '../hooks/useTranslations'

export default function Privacy() {
  const { t } = useTranslations()

  return (
    <div className="card legal-page">
      <h1 className="page-title">{t('privacy_title')}</h1>
      <p className="legal-updated">{t('privacy_updated')}</p>

      <section>
        <h2>{t('privacy_intro_heading')}</h2>
        <p>{t('privacy_intro_body')}</p>
      </section>

      <section>
        <h2>{t('privacy_data_heading')}</h2>
        <p>{t('privacy_data_body')}</p>
      </section>

      <section>
        <h2>{t('privacy_use_heading')}</h2>
        <p>{t('privacy_use_body')}</p>
      </section>

      <section>
        <h2>{t('privacy_security_heading')}</h2>
        <p>{t('privacy_security_body')}</p>
      </section>

      <section>
        <h2>{t('privacy_contact_heading')}</h2>
        <p>{t('privacy_contact_body')}</p>
      </section>

      <p style={{ marginTop: '2rem' }}>
        <Link to="/" className="link">{t('common_back')}</Link>
      </p>
    </div>
  )
}
