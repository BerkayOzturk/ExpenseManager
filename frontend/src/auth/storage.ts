const TOKEN_KEY = 'expense_manager_token'
const USER_KEY = 'expense_manager_user'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setAuth(token: string, email: string): void {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USER_KEY, email)
}

export function clearAuth(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}

export function getStoredEmail(): string | null {
  return localStorage.getItem(USER_KEY)
}
