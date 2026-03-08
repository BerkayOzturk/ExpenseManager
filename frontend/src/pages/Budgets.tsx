import { useState, useEffect } from 'react'
import { api } from '../api/client'
import { useSettings } from '../contexts/SettingsContext'
import { useTranslations } from '../hooks/useTranslations'
import type { BudgetSummary, Category } from '../types'

const CURRENCIES = ['USD', 'EUR', 'GBP', 'TRY', 'CHF', 'JPY', 'CAD', 'AUD'] as const
const MONTHS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12] as const
const currentYear = new Date().getFullYear()

function periodLabel(year: number, month: number | null): string {
  if (month != null) {
    const name = new Date(2000, month - 1, 1).toLocaleString('default', { month: 'short' })
    return `${name} ${year}`
  }
  return `Year ${year}`
}

export default function Budgets() {
  const { settings } = useSettings()
  const { t } = useTranslations()
  const [summary, setSummary] = useState<BudgetSummary[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [categoryId, setCategoryId] = useState<string>('')
  const [amount, setAmount] = useState('')
  const [currency, setCurrency] = useState(settings.defaultCurrency)
  const [year, setYear] = useState(currentYear)
  const [month, setMonth] = useState<number | ''>('')

  const [editingId, setEditingId] = useState<string | null>(null)
  const [editAmount, setEditAmount] = useState('')
  const [editCurrency, setEditCurrency] = useState('USD')

  const loadSummary = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await api.budgets.summary()
      setSummary(data)
    } catch (e) {
      setError(e instanceof Error ? e.message : t('budgets_failed_load'))
      setSummary([])
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
    loadSummary()
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
      await api.budgets.create({
        categoryId: categoryId || null,
        amount: num,
        currency,
        year,
        month: month === '' ? null : month,
      })
      setAmount('')
      setMonth('')
      await loadSummary()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('budgets_failed_create'))
    }
  }

  const startEdit = (b: BudgetSummary) => {
    setEditingId(b.id)
    setEditAmount(String(b.budgetAmount))
    setEditCurrency(b.currency)
  }

  const cancelEdit = () => {
    setEditingId(null)
    setEditAmount('')
    setEditCurrency('USD')
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingId) return
    const num = parseFloat(editAmount)
    if (Number.isNaN(num) || num < 0) return
    setError(null)
    try {
      await api.budgets.update(editingId, { amount: num, currency: editCurrency })
      cancelEdit()
      await loadSummary()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('budgets_failed_update'))
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm(t('budgets_delete_confirm'))) return
    setError(null)
    try {
      await api.budgets.delete(id)
      if (editingId === id) cancelEdit()
      await loadSummary()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('budgets_failed_delete'))
    }
  }

  if (loading) return <p className="loading">{t('common_loading')}</p>

  return (
    <div className="page">
      <h1>{t('budgets_title')}</h1>
      <p className="muted">{t('budgets_description')}</p>

      {error && <div className="error">{error}</div>}

      <section className="card">
        <h2>{t('budgets_add')}</h2>
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <label>
              {t('budgets_scope')}
              <select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value)}
                aria-label={t('budgets_scope')}
              >
                <option value="">{t('budgets_scope_total')}</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </label>
            <label>
              {t('expenses_currency')}
              <select value={currency} onChange={(e) => setCurrency(e.target.value)}>
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </label>
          </div>
          <div className="form-row">
            <label>
              {t('budgets_amount')}
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
              {t('budgets_year')}
              <input
                type="number"
                min="2000"
                max="2100"
                value={year}
                onChange={(e) => setYear(parseInt(e.target.value, 10) || currentYear)}
              />
            </label>
            <label>
              {t('budgets_month')} <span className="muted">{t('budgets_month_whole_year')}</span>
              <select
                value={month === '' ? '' : month}
                onChange={(e) => setMonth(e.target.value === '' ? '' : Number(e.target.value))}
              >
                <option value="">—</option>
                {MONTHS.map((m) => (
                  <option key={m} value={m}>{new Date(2000, m - 1, 1).toLocaleString('default', { month: 'long' })}</option>
                ))}
              </select>
            </label>
          </div>
          <button type="submit" className="btn btn-primary">{t('budgets_add')}</button>
        </form>
      </section>

      <section className="card">
        <h2>{t('budgets_vs_spent')}</h2>
        {summary.length === 0 ? (
          <p className="muted">{t('budgets_no_budgets')}</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('budgets_period')}</th>
                  <th>{t('budgets_scope_label')}</th>
                  <th>{t('expenses_currency')}</th>
                  <th>{t('budgets_budget')}</th>
                  <th>{t('budgets_spent')}</th>
                  <th>{t('budgets_status')}</th>
                  <th aria-label={t('common_actions')} />
                </tr>
              </thead>
              <tbody>
                {summary.map((row) => (
                  <tr key={row.id} className={row.isOverBudget ? 'over-budget' : ''}>
                    <td data-label={t('budgets_period')}>{periodLabel(row.year, row.month)}</td>
                    <td data-label={t('budgets_scope_label')}>{row.categoryName ?? t('budgets_total')}</td>
                    <td data-label={t('expenses_currency')}>{row.currency}</td>
                    <td data-label={t('budgets_budget')}>{row.budgetAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                    <td data-label={t('budgets_spent')}>{row.spent.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                    <td data-label={t('budgets_status')}>
                      {row.isOverBudget ? (
                        <span className="badge badge-danger">{t('budgets_over_budget')}</span>
                      ) : (
                        <span className="badge badge-ok">{t('budgets_ok')}</span>
                      )}
                    </td>
                    <td className="actions-cell" data-label={t('common_actions')}>
                      {editingId === row.id ? (
                        <form onSubmit={handleUpdate} className="inline-form">
                          <input
                            type="number"
                            min="0"
                            step="any"
                            value={editAmount}
                            onChange={(e) => setEditAmount(e.target.value)}
                            className="input-sm"
                            aria-label={t('budgets_amount')}
                          />
                          <select
                            value={editCurrency}
                            onChange={(e) => setEditCurrency(e.target.value)}
                            className="input-sm"
                            aria-label={t('expenses_currency')}
                          >
                            {CURRENCIES.map((c) => (
                              <option key={c} value={c}>{c}</option>
                            ))}
                          </select>
                          <button type="submit" className="btn btn-sm btn-primary">{t('common_save')}</button>
                          <button type="button" className="btn btn-sm btn-secondary" onClick={cancelEdit}>{t('common_cancel')}</button>
                        </form>
                      ) : (
                        <>
                          <button type="button" className="btn btn-sm btn-secondary" onClick={() => startEdit(row)}>{t('common_edit')}</button>
                          <button type="button" className="btn btn-sm btn-danger" onClick={() => handleDelete(row.id)}>{t('common_delete')}</button>
                        </>
                      )}
                    </td>
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
