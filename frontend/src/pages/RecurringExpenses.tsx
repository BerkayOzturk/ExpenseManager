import { useState, useEffect } from 'react'
import { api } from '../api/client'
import { useSettings } from '../contexts/SettingsContext'
import { useTranslations } from '../hooks/useTranslations'
import type { Category, RecurringExpense, UpcomingExpense } from '../types'

const CURRENCIES = ['USD', 'EUR', 'GBP', 'TRY', 'CHF', 'JPY', 'CAD', 'AUD'] as const

export default function RecurringExpenses() {
  const { settings } = useSettings()
  const { t } = useTranslations()
  const [list, setList] = useState<RecurringExpense[]>([])
  const [upcoming, setUpcoming] = useState<UpcomingExpense[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [amount, setAmount] = useState('')
  const [currency, setCurrency] = useState(settings.defaultCurrency)
  const [firstOccurrenceOn, setFirstOccurrenceOn] = useState(() => new Date().toISOString().slice(0, 10))
  const [description, setDescription] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [endOn, setEndOn] = useState('')

  const [editingId, setEditingId] = useState<string | null>(null)
  const [editAmount, setEditAmount] = useState('')
  const [editCurrency, setEditCurrency] = useState('USD')
  const [editFirstOccurrenceOn, setEditFirstOccurrenceOn] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editCategoryId, setEditCategoryId] = useState('')
  const [editEndOn, setEditEndOn] = useState('')

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const [listData, upcomingData] = await Promise.all([
        api.recurringExpenses.list(),
        api.recurringExpenses.upcoming(3),
      ])
      setList(listData)
      setUpcoming(upcomingData)
    } catch (e) {
      setError(e instanceof Error ? e.message : t('recurring_failed_load'))
      setList([])
      setUpcoming([])
    } finally {
      setLoading(false)
    }
  }

  const loadCategories = async () => {
    try {
      const data = await api.categories.list()
      setCategories(data)
    } catch {
      setCategories([])
    }
  }

  useEffect(() => {
    load()
    loadCategories()
    // eslint-disable-next-line react-hooks/exhaustive-deps -- run once on mount
  }, [])

  useEffect(() => {
    setCurrency(settings.defaultCurrency)
  }, [settings.defaultCurrency])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    const num = parseFloat(amount)
    if (Number.isNaN(num) || num < 0) return
    setError(null)
    try {
      await api.recurringExpenses.create({
        amount: num,
        currency,
        firstOccurrenceOn,
        description: description.trim() || null,
        categoryId: categoryId || null,
        endOn: endOn || null,
      })
      setAmount('')
      setDescription('')
      setCategoryId('')
      setEndOn('')
      setFirstOccurrenceOn(new Date().toISOString().slice(0, 10))
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('recurring_failed_create'))
    }
  }

  const startEdit = (r: RecurringExpense) => {
    setEditingId(r.id)
    setEditAmount(String(r.amount))
    setEditCurrency(r.currency)
    setEditFirstOccurrenceOn(r.firstOccurrenceOn)
    setEditDescription(r.description ?? '')
    setEditCategoryId(r.categoryId ?? '')
    setEditEndOn(r.endOn ?? '')
  }

  const cancelEdit = () => {
    setEditingId(null)
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingId) return
    const num = parseFloat(editAmount)
    if (Number.isNaN(num) || num < 0) return
    setError(null)
    try {
      await api.recurringExpenses.update(editingId, {
        amount: num,
        currency: editCurrency,
        firstOccurrenceOn: editFirstOccurrenceOn,
        description: editDescription.trim() || null,
        categoryId: editCategoryId || null,
        endOn: editEndOn || null,
      })
      cancelEdit()
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('recurring_failed_update'))
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm(t('recurring_delete_confirm'))) return
    setError(null)
    try {
      await api.recurringExpenses.delete(id)
      if (editingId === id) cancelEdit()
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('recurring_failed_delete'))
    }
  }

  if (loading) return <p className="loading">{t('common_loading')}</p>

  return (
    <div className="page">
      <h1>{t('recurring_title')}</h1>
      <p className="muted">{t('recurring_description')}</p>

      {error && <div className="error">{error}</div>}

      <section className="card">
        <h2>{t('recurring_add')}</h2>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <label>
              {t('expenses_amount')}
              <input
                type="number"
                min="0"
                step="any"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder="0"
              />
            </label>
            <label>
              {t('expenses_currency')}
              <select value={currency} onChange={(e) => setCurrency(e.target.value)}>
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </label>
            <label>
              {t('recurring_first_date')}
              <input
                type="date"
                value={firstOccurrenceOn}
                onChange={(e) => setFirstOccurrenceOn(e.target.value)}
              />
            </label>
            <label>
              {t('recurring_end_date')}
              <input
                type="date"
                value={endOn}
                onChange={(e) => setEndOn(e.target.value)}
                placeholder="Optional"
              />
            </label>
          </div>
          <div className="form-row">
            <label>
              {t('expenses_category')}
              <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
                <option value="">{t('expenses_none')}</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </label>
            <label>
              {t('expenses_description')}
              <input
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder={t('expenses_optional')}
              />
            </label>
            <button type="submit" className="btn btn-primary">{t('common_add')}</button>
          </div>
        </form>
      </section>

      <section className="card">
        <h2>{t('recurring_title')}</h2>
        {list.length === 0 ? (
          <p className="muted">{t('recurring_no_list')}</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('expenses_amount')}</th>
                  <th>{t('expenses_currency')}</th>
                  <th>{t('recurring_first_date')}</th>
                  <th>{t('recurring_end_date')}</th>
                  <th>{t('expenses_category')}</th>
                  <th>{t('expenses_description')}</th>
                  <th style={{ width: 120 }}>{t('common_actions')}</th>
                </tr>
              </thead>
              <tbody>
                {list.map((r) => (
                  <tr key={r.id}>
                    {editingId === r.id ? (
                      <>
                        <td data-label={t('expenses_amount')}>
                          <input
                            type="number"
                            min="0"
                            step="any"
                            value={editAmount}
                            onChange={(e) => setEditAmount(e.target.value)}
                            className="input-sm"
                            style={{ width: 80 }}
                          />
                        </td>
                        <td data-label={t('expenses_currency')}>
                          <select value={editCurrency} onChange={(e) => setEditCurrency(e.target.value)} className="input-sm">
                            {CURRENCIES.map((c) => (
                              <option key={c} value={c}>{c}</option>
                            ))}
                          </select>
                        </td>
                        <td data-label={t('recurring_first_date')}>
                          <input
                            type="date"
                            value={editFirstOccurrenceOn}
                            onChange={(e) => setEditFirstOccurrenceOn(e.target.value)}
                            className="input-sm"
                          />
                        </td>
                        <td data-label={t('recurring_end_date')}>
                          <input
                            type="date"
                            value={editEndOn}
                            onChange={(e) => setEditEndOn(e.target.value)}
                            className="input-sm"
                          />
                        </td>
                        <td data-label={t('expenses_category')}>
                          <select value={editCategoryId} onChange={(e) => setEditCategoryId(e.target.value)} className="input-sm">
                            <option value="">{t('expenses_none')}</option>
                            {categories.map((c) => (
                              <option key={c.id} value={c.id}>{c.name}</option>
                            ))}
                          </select>
                        </td>
                        <td data-label={t('expenses_description')}>
                          <input
                            value={editDescription}
                            onChange={(e) => setEditDescription(e.target.value)}
                            className="input-sm"
                            style={{ minWidth: 100 }}
                          />
                        </td>
                        <td className="actions-cell" data-label={t('common_actions')}>
                          <form onSubmit={handleUpdate} className="inline-form">
                            <button type="submit" className="btn btn-sm btn-primary">{t('common_save')}</button>
                            <button type="button" className="btn btn-sm btn-secondary" onClick={cancelEdit}>{t('common_cancel')}</button>
                          </form>
                        </td>
                      </>
                    ) : (
                      <>
                        <td data-label={t('expenses_amount')}>{r.amount.toFixed(2)}</td>
                        <td data-label={t('expenses_currency')}>{r.currency}</td>
                        <td data-label={t('recurring_first_date')}>{r.firstOccurrenceOn}</td>
                        <td data-label={t('recurring_end_date')}>{r.endOn ?? '—'}</td>
                        <td data-label={t('expenses_category')}>{r.categoryName ?? '—'}</td>
                        <td data-label={t('expenses_description')}>{r.description ?? '—'}</td>
                        <td className="actions-cell" data-label={t('common_actions')}>
                          <button type="button" className="btn btn-sm btn-secondary" onClick={() => startEdit(r)}>{t('common_edit')}</button>
                          <button type="button" className="btn btn-sm btn-danger" onClick={() => handleDelete(r.id)}>{t('common_delete')}</button>
                        </td>
                      </>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="card">
        <h2>{t('recurring_upcoming')}</h2>
        <p className="muted" style={{ marginBottom: '1rem' }}>{t('recurring_upcoming_hint')}</p>
        {upcoming.length === 0 ? (
          <p className="muted">{t('recurring_no_upcoming')}</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('expenses_date')}</th>
                  <th>{t('expenses_amount')}</th>
                  <th>{t('expenses_currency')}</th>
                  <th>{t('expenses_category')}</th>
                  <th>{t('expenses_description')}</th>
                </tr>
              </thead>
              <tbody>
                {upcoming.map((u, i) => (
                  <tr key={`${u.occurredOn}-${u.recurringExpenseId}-${i}`}>
                    <td data-label={t('expenses_date')}>{u.occurredOn}</td>
                    <td data-label={t('expenses_amount')}>{u.amount.toFixed(2)}</td>
                    <td data-label={t('expenses_currency')}>{u.currency}</td>
                    <td data-label={t('expenses_category')}>{u.categoryName ?? '—'}</td>
                    <td data-label={t('expenses_description')}>{u.description ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
