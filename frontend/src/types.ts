export interface Category {
  id: string
  name: string
}

export interface Expense {
  id: string
  amount: number
  currency: string
  occurredOn: string
  description: string | null
  categoryId: string | null
  categoryName: string | null
}

export interface CreateCategoryRequest {
  name: string
}

export interface UpdateCategoryRequest {
  name: string
}

export interface CreateExpenseRequest {
  amount: number
  currency: string
  occurredOn: string
  description?: string | null
  categoryId?: string | null
}

export interface UpdateExpenseRequest extends CreateExpenseRequest {}

export interface Budget {
  id: string
  categoryId: string | null
  categoryName: string | null
  amount: number
  currency: string
  year: number
  month: number | null
}

export interface BudgetSummary {
  id: string
  categoryId: string | null
  categoryName: string | null
  budgetAmount: number
  currency: string
  year: number
  month: number | null
  spent: number
  isOverBudget: boolean
}

export interface UserSettings {
  defaultCurrency: string
  dateFormat: string
  theme: 'light' | 'dark' | 'system'
  language: string | null
}

export interface RecurringExpense {
  id: string
  amount: number
  currency: string
  firstOccurrenceOn: string
  description: string | null
  categoryId: string | null
  categoryName: string | null
  endOn: string | null
}

export interface UpcomingExpense {
  occurredOn: string
  amount: number
  currency: string
  description: string | null
  categoryName: string | null
  recurringExpenseId: string
}
