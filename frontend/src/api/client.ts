import { getToken } from '../auth/storage'
import type { Budget, BudgetSummary, Category, Expense, RecurringExpense, UpcomingExpense, UserSettings } from '../types'

const BASE = '/api'

async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...options.headers,
  }
  const token = getToken()
  if (token) {
    (headers as Record<string, string>)['Authorization'] = `Bearer ${token}`
  }

  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers,
  })

  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    const message = body.detail ?? body.title ?? res.statusText
    throw new Error(message)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

export const api = {
  categories: {
    list: () => request<Category[]>('/categories'),
    get: (id: string) => request<Category>(`/categories/${id}`),
    create: (body: { name: string }) =>
      request<Category>('/categories', {
        method: 'POST',
        body: JSON.stringify(body),
      }),
    update: (id: string, body: { name: string }) =>
      request<Category>(`/categories/${id}`, {
        method: 'PUT',
        body: JSON.stringify(body),
      }),
    delete: (id: string) =>
      request<void>(`/categories/${id}`, { method: 'DELETE' }),
  },
  expenses: {
    list: (params?: { from?: string; to?: string; categoryId?: string }) => {
      const search = new URLSearchParams()
      if (params?.from) search.set('from', params.from)
      if (params?.to) search.set('to', params.to)
      if (params?.categoryId) search.set('categoryId', params.categoryId)
      const q = search.toString()
      return request<Expense[]>(`/expenses${q ? `?${q}` : ''}`)
    },
    get: (id: string) => request<Expense>(`/expenses/${id}`),
    create: (body: CreateExpenseBody) =>
      request<Expense>('/expenses', {
        method: 'POST',
        body: JSON.stringify(body),
      }),
    update: (id: string, body: CreateExpenseBody) =>
      request<Expense>(`/expenses/${id}`, {
        method: 'PUT',
        body: JSON.stringify(body),
      }),
    delete: (id: string) =>
      request<void>(`/expenses/${id}`, { method: 'DELETE' }),
  },
  budgets: {
    list: () => request<Budget[]>('/budgets'),
    summary: () => request<BudgetSummary[]>('/budgets/summary'),
    get: (id: string) => request<Budget>(`/budgets/${id}`),
    create: (body: { categoryId?: string | null; amount: number; currency: string; year: number; month?: number | null }) =>
      request<Budget>('/budgets', {
        method: 'POST',
        body: JSON.stringify(body),
      }),
    update: (id: string, body: { amount: number; currency: string }) =>
      request<Budget>(`/budgets/${id}`, {
        method: 'PUT',
        body: JSON.stringify(body),
      }),
    delete: (id: string) =>
      request<void>(`/budgets/${id}`, { method: 'DELETE' }),
  },
  settings: {
    get: () => request<UserSettings>('/settings'),
    update: (body: { defaultCurrency: string; dateFormat: string; theme: string; language?: string | null }) =>
      request<UserSettings>('/settings', {
        method: 'PUT',
        body: JSON.stringify(body),
      }),
  },
  recurringExpenses: {
    list: () => request<RecurringExpense[]>('/recurring-expenses'),
    upcoming: (months = 3) =>
      request<UpcomingExpense[]>(`/recurring-expenses/upcoming?months=${months}`),
    create: (body: { amount: number; currency: string; firstOccurrenceOn: string; description?: string | null; categoryId?: string | null; endOn?: string | null }) =>
      request<RecurringExpense>('/recurring-expenses', {
        method: 'POST',
        body: JSON.stringify(body),
      }),
    update: (id: string, body: { amount: number; currency: string; firstOccurrenceOn: string; description?: string | null; categoryId?: string | null; endOn?: string | null }) =>
      request<RecurringExpense>(`/recurring-expenses/${id}`, {
        method: 'PUT',
        body: JSON.stringify(body),
      }),
    delete: (id: string) =>
      request<void>(`/recurring-expenses/${id}`, { method: 'DELETE' }),
  },
  admin: {
    /** Requires X-Admin-Key header. Secret from env (Admin__SecretKey); never in repo. */
    users: (adminKey: string) =>
      fetch(`${BASE}/admin/users`, {
        headers: { 'X-Admin-Key': adminKey },
      }).then(async (res) => {
        if (!res.ok) {
          const body = await res.json().catch(() => ({}))
          throw new Error(body.detail ?? res.statusText)
        }
        return res.json() as Promise<AdminUserSummary[]>
      }),
  },
}

export interface AdminUserSummary {
  userId: string
  email: string
  expenseCount: number
  categoryCount: number
}

interface CreateExpenseBody {
  amount: number
  currency: string
  occurredOn: string
  description?: string | null
  categoryId?: string | null
}
