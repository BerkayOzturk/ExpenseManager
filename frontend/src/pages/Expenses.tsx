import { useState, useEffect, useMemo } from 'react'
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts'
import { api } from '../api/client'
import { useSettings } from '../contexts/SettingsContext'
import { useTranslations } from '../hooks/useTranslations'
import type { Category, Expense } from '../types'

const CURRENCIES = ['USD', 'EUR', 'GBP', 'TRY', 'CHF', 'JPY', 'CAD', 'AUD'] as const

const CHART_COLORS = [
  '#6366f1', '#8b5cf6', '#a855f7', '#d946ef', '#ec4899',
  '#f43f5e', '#f97316', '#eab308', '#84cc16', '#22c55e',
  '#14b8a6', '#06b6d4', '#0ea5e9', '#3b82f6', '#64748b',
]

function getColorForCategory(categoryName: string, allNames: string[]): string {
  const index = allNames.indexOf(categoryName)
  return CHART_COLORS[index % CHART_COLORS.length]
}

export default function Expenses() {
  const { settings } = useSettings()
  const { t, locale } = useTranslations()
  const [expenses, setExpenses] = useState<Expense[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [filterFrom, setFilterFrom] = useState('')
  const [filterTo, setFilterTo] = useState('')
  const [filterCategoryId, setFilterCategoryId] = useState('')

  const [amount, setAmount] = useState('')
  const [currency, setCurrency] = useState(settings.defaultCurrency)
  const [occurredOn, setOccurredOn] = useState(() => new Date().toISOString().slice(0, 10))
  const [description, setDescription] = useState('')
  const [categoryId, setCategoryId] = useState('')

  const [showFilters, setShowFilters] = useState(false)
  const [showAddExpense, setShowAddExpense] = useState(false)

  const [editingId, setEditingId] = useState<string | null>(null)
  const [editAmount, setEditAmount] = useState('')
  const [editCurrency, setEditCurrency] = useState('USD')
  const [editOccurredOn, setEditOccurredOn] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editCategoryId, setEditCategoryId] = useState('')

  const loadCategories = async () => {
    try {
      const data = await api.categories.list()
      setCategories(data)
    } catch {
      setCategories([])
    }
  }

  const loadExpenses = async () => {
    setLoading(true)
    setError(null)
    try {
      const params: { from?: string; to?: string; categoryId?: string } = {}
      if (filterFrom) params.from = filterFrom
      if (filterTo) params.to = filterTo
      if (filterCategoryId) params.categoryId = filterCategoryId
      const data = await api.expenses.list(params)
      setExpenses(data)
    } catch (e) {
      setError(e instanceof Error ? e.message : t('expenses_failed_load'))
      setExpenses([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadCategories()
  }, [])

  useEffect(() => {
    setCurrency(settings.defaultCurrency)
  }, [settings.defaultCurrency])

  const filterFromValid = !filterFrom || !filterTo || filterFrom <= filterTo

  function toYMD(d: Date): string {
    return d.toISOString().slice(0, 10)
  }
  const setQuickRange = (from: Date, to: Date) => {
    setFilterFrom(toYMD(from))
    setFilterTo(toYMD(to))
  }
  const applyThisMonth = () => {
    const now = new Date()
    setQuickRange(new Date(now.getFullYear(), now.getMonth(), 1), new Date(now.getFullYear(), now.getMonth() + 1, 0))
  }
  const applyLastMonth = () => {
    const now = new Date()
    setQuickRange(new Date(now.getFullYear(), now.getMonth() - 1, 1), new Date(now.getFullYear(), now.getMonth(), 0))
  }
  const applyThisYear = () => {
    const y = new Date().getFullYear()
    setQuickRange(new Date(y, 0, 1), new Date(y, 11, 31))
  }

  const chartDataByCurrency = useMemo(() => {
    const byCurrency: Record<string, Record<string, number>> = {}
    const allCategoryNames: string[] = []
    const seenCategories = new Set<string>()

    for (const x of expenses) {
      const cat = x.categoryName?.trim() || t('expenses_uncategorized')
      if (!seenCategories.has(cat)) {
        seenCategories.add(cat)
        allCategoryNames.push(cat)
      }
      if (!byCurrency[x.currency]) byCurrency[x.currency] = {}
      byCurrency[x.currency][cat] = (byCurrency[x.currency][cat] ?? 0) + Number(x.amount)
    }

    allCategoryNames.sort()

    return Object.entries(byCurrency).map(([currency, byCat]) => ({
      currency,
      data: Object.entries(byCat).map(([name, value]) => ({
        name,
        value: Math.round(value * 100) / 100,
        fill: getColorForCategory(name, allCategoryNames),
      })),
    }))
  }, [expenses, t])

  const spendingOverTimeByCurrency = useMemo(() => {
    const byCurrency: Record<string, Record<string, number>> = {}
    for (const x of expenses) {
      const [y, m] = x.occurredOn.split('-')
      const monthKey = `${y}-${m}`
      if (!byCurrency[x.currency]) byCurrency[x.currency] = {}
      byCurrency[x.currency][monthKey] = (byCurrency[x.currency][monthKey] ?? 0) + Number(x.amount)
    }
    const localeTag = locale === 'tr' ? 'tr-TR' : 'en'
    const monthNames = new Intl.DateTimeFormat(localeTag, { month: 'short', year: 'numeric' })
    return Object.entries(byCurrency).map(([currency, byMonth]) => {
      const sorted = Object.entries(byMonth)
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([monthKey, total]) => {
          const [y, m] = monthKey.split('-').map(Number)
          const date = new Date(y, m - 1, 1)
          return { monthKey, label: monthNames.format(date), total: Math.round(total * 100) / 100 }
        })
      return { currency, data: sorted }
    })
  }, [expenses, locale])

  useEffect(() => {
    if (!filterFromValid) return
    loadExpenses()
  }, [filterFrom, filterTo, filterCategoryId, filterFromValid])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    const num = parseFloat(amount)
    if (Number.isNaN(num) || num < 0) return
    setError(null)
    try {
      await api.expenses.create({
        amount: num,
        currency: currency.trim() || 'USD',
        occurredOn: occurredOn,
        description: description.trim() || null,
        categoryId: categoryId || null,
      })
      setAmount('')
      setDescription('')
      setCategoryId('')
      setOccurredOn(new Date().toISOString().slice(0, 10))
      await loadExpenses()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('common_add'))
    }
  }

  const startEdit = (x: Expense) => {
    setEditingId(x.id)
    setEditAmount(String(x.amount))
    setEditCurrency(x.currency)
    setEditOccurredOn(x.occurredOn)
    setEditDescription(x.description ?? '')
    setEditCategoryId(x.categoryId ?? '')
  }

  const cancelEdit = () => {
    setEditingId(null)
  }

  const handleUpdate = async (e?: React.FormEvent) => {
    e?.preventDefault()
    if (!editingId) return
    const num = parseFloat(editAmount)
    if (Number.isNaN(num) || num < 0) return
    setError(null)
    try {
      await api.expenses.update(editingId, {
        amount: num,
        currency: editCurrency.trim() || 'USD',
        occurredOn: editOccurredOn,
        description: editDescription.trim() || null,
        categoryId: editCategoryId || null,
      })
      cancelEdit()
      await loadExpenses()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('common_save'))
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm(t('expenses_delete_confirm'))) return
    setError(null)
    try {
      await api.expenses.delete(id)
      if (editingId === id) cancelEdit()
      await loadExpenses()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to delete')
    }
  }

  return (
    <>
      <h1 className="page-title">{t('expenses_title')}</h1>

      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem', marginBottom: '1rem' }}>
        <button
          type="button"
          className="btn btn-secondary"
          onClick={() => setShowFilters((v) => !v)}
          aria-expanded={showFilters}
        >
          {showFilters ? t('expenses_hide_filters') : t('expenses_show_filters')}
        </button>
        <button
          type="button"
          className="btn btn-primary"
          onClick={() => setShowAddExpense((v) => !v)}
          aria-expanded={showAddExpense}
        >
          {showAddExpense ? t('common_cancel') : t('expenses_add')}
        </button>
      </div>

      {showFilters && (
        <div className="card">
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t('expenses_filters')}</h2>
          <div className="filters">
            <div className="form-group">
              <label>{t('expenses_filter_from')}</label>
              <input
                type="date"
                value={filterFrom}
                max={filterTo || undefined}
                onChange={(e) => setFilterFrom(e.target.value)}
              />
            </div>
            <div className="form-group">
              <label>{t('expenses_filter_to')}</label>
              <input
                type="date"
                value={filterTo}
                min={filterFrom || undefined}
                onChange={(e) => setFilterTo(e.target.value)}
              />
            </div>
            <div className="form-group">
              <label>{t('expenses_filter_category')}</label>
              <select
                value={filterCategoryId}
                onChange={(e) => setFilterCategoryId(e.target.value)}
              >
                <option value="">{t('expenses_all')}</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
              <span style={{ fontSize: '0.875rem', color: '#64748b', marginRight: '0.25rem' }}>{t('expenses_quick')}:</span>
              <button type="button" className="btn btn-secondary btn-sm" onClick={applyThisMonth}>
                {t('expenses_quick_this_month')}
              </button>
              <button type="button" className="btn btn-secondary btn-sm" onClick={applyLastMonth}>
                {t('expenses_quick_last_month')}
              </button>
              <button type="button" className="btn btn-secondary btn-sm" onClick={applyThisYear}>
                {t('expenses_quick_this_year')}
              </button>
            </div>
          </div>
          {!filterFromValid && (
            <p className="error-msg">{t('expenses_invalid_dates')}</p>
          )}
        </div>
      )}

      {showAddExpense && (
        <div className="card">
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t('expenses_add')}</h2>
          <form onSubmit={handleCreate}>
            <div className="form-row">
              <div className="form-group">
                <label>{t('expenses_amount')}</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  placeholder="0.00"
                />
              </div>
              <div className="form-group">
                <label>{t('expenses_currency')}</label>
                <select
                  value={currency}
                  onChange={(e) => setCurrency(e.target.value)}
                >
                  {CURRENCIES.map((c) => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>{t('expenses_date')}</label>
                <input
                  type="date"
                  value={occurredOn}
                  onChange={(e) => setOccurredOn(e.target.value)}
                />
              </div>
              <div className="form-group">
                <label>{t('expenses_category')}</label>
                <select
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                >
                  <option value="">{t('expenses_none')}</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>{t('expenses_description')}</label>
                <input
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder={t('expenses_optional')}
                />
              </div>
              <button type="submit" className="btn btn-primary">{t('common_add')}</button>
            </div>
          </form>
        </div>
      )}

      {chartDataByCurrency.length > 0 && (
        <div className="card">
          <h2 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t('expenses_by_category')}</h2>
          <p style={{ margin: '0 0 1rem', color: '#64748b', fontSize: '0.875rem' }}>
            {t('expenses_chart_hint')}
          </p>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '2rem', justifyContent: 'flex-start' }}>
            {chartDataByCurrency.map(({ currency, data }) => (
              <div key={currency} style={{ minWidth: 280, maxWidth: 380, height: 320 }}>
                <h3 style={{ margin: '0 0 0.5rem', fontSize: '0.95rem', fontWeight: 600 }}>
                  {currency}
                </h3>
                <ResponsiveContainer width="100%" height={280}>
                  <PieChart margin={{ top: 4, right: 4, bottom: 4, left: 4 }}>
                    <Pie
                      data={data}
                      dataKey="value"
                      nameKey="name"
                      cx="50%"
                      cy="50%"
                      outerRadius={82}
                      label={({ percent }: { percent?: number }) => `${((percent ?? 0) * 100).toFixed(0)}%`}
                      labelLine={false}
                    >
                      {data.map((entry, index) => (
                        <Cell key={index} fill={entry.fill} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value: number) => [value.toFixed(2), 'Total']} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            ))}
          </div>
        </div>
      )}

      {spendingOverTimeByCurrency.some(({ data }) => data.length > 0) && (
        <div className="card">
          <h2 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t('expenses_spending_over_time')}</h2>
          <p style={{ margin: '0 0 1rem', color: '#64748b', fontSize: '0.875rem' }}>
            {t('expenses_spending_over_time_hint')}
          </p>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '2rem', justifyContent: 'flex-start' }}>
            {spendingOverTimeByCurrency.map(({ currency, data }) => (
              <div key={currency} style={{ minWidth: 280, maxWidth: 520, height: 300 }}>
                <h3 style={{ margin: '0 0 0.5rem', fontSize: '0.95rem', fontWeight: 600 }}>
                  {currency}
                </h3>
                <ResponsiveContainer width="100%" height={260}>
                  <BarChart data={data} margin={{ top: 8, right: 8, bottom: 24, left: 8 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                    <XAxis dataKey="label" tick={{ fontSize: 12 }} />
                    <YAxis tick={{ fontSize: 12 }} tickFormatter={(v) => String(v)} />
                    <Tooltip formatter={(value: number) => [value.toFixed(2), t('expenses_amount')]} />
                    <Bar dataKey="total" fill="#6366f1" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            ))}
          </div>
        </div>
      )}

      {error && <p className="error-msg">{error}</p>}

      <div className="card">
        {loading ? (
          <p className="empty">{t('common_loading')}</p>
        ) : expenses.length === 0 ? (
          <p className="empty">{t('expenses_no_expenses')}</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('expenses_date')}</th>
                  <th>{t('expenses_amount')}</th>
                  <th>{t('expenses_category')}</th>
                  <th>{t('expenses_description')}</th>
                  <th style={{ width: 140 }}>{t('common_actions')}</th>
                </tr>
              </thead>
              <tbody>
                {expenses.map((x) => (
                  <tr key={x.id}>
                    <td data-label={t('expenses_date')}>
                      {editingId === x.id ? (
                        <input
                          type="date"
                          value={editOccurredOn}
                          onChange={(e) => setEditOccurredOn(e.target.value)}
                        />
                      ) : (
                        x.occurredOn
                      )}
                    </td>
                    <td data-label={t('expenses_amount')}>
                      {editingId === x.id ? (
                        <>
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={editAmount}
                            onChange={(e) => setEditAmount(e.target.value)}
                            style={{ width: 80 }}
                          />
                          <select
                            value={editCurrency}
                            onChange={(e) => setEditCurrency(e.target.value)}
                            style={{ width: 72, marginLeft: 4 }}
                          >
                            {!CURRENCIES.includes(editCurrency as (typeof CURRENCIES)[number]) && (
                              <option value={editCurrency}>{editCurrency}</option>
                            )}
                            {CURRENCIES.map((c) => (
                              <option key={c} value={c}>{c}</option>
                            ))}
                          </select>
                        </>
                      ) : (
                        `${x.currency} ${Number(x.amount).toFixed(2)}`
                      )}
                    </td>
                    <td data-label={t('expenses_category')}>
                      {editingId === x.id ? (
                        <select
                          value={editCategoryId}
                          onChange={(e) => setEditCategoryId(e.target.value)}
                        >
                          <option value="">{t('expenses_none')}</option>
                          {categories.map((c) => (
                            <option key={c.id} value={c.id}>{c.name}</option>
                          ))}
                        </select>
                      ) : (
                        x.categoryName ?? t('expenses_uncategorized')
                      )}
                    </td>
                    <td data-label={t('expenses_description')}>
                      {editingId === x.id ? (
                        <input
                          value={editDescription}
                          onChange={(e) => setEditDescription(e.target.value)}
                          placeholder={t('expenses_optional')}
                          style={{ minWidth: 120 }}
                        />
                      ) : (
                        x.description ?? '—'
                      )}
                    </td>
                    <td className="actions-cell" data-label={t('common_actions')}>
                      {editingId === x.id ? (
                        <div className="actions">
                          <button type="button" className="btn btn-primary btn-sm" onClick={() => handleUpdate()}>{t('common_save')}</button>
                          <button type="button" className="btn btn-secondary btn-sm" onClick={cancelEdit}>{t('common_cancel')}</button>
                        </div>
                      ) : (
                        <div className="actions">
                          <button type="button" className="btn btn-secondary btn-sm" onClick={() => startEdit(x)}>{t('common_edit')}</button>
                          <button type="button" className="btn btn-danger btn-sm" onClick={() => handleDelete(x.id)}>{t('common_delete')}</button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </>
  )
}
