import { clearAuth, setAuth } from '../auth/storage'

export interface AuthResponse {
  userId: string
  email: string
  token: string
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const res = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    throw new Error(body.detail ?? body.title ?? 'Login failed')
  }
  const data = (await res.json()) as AuthResponse
  setAuth(data.token, data.email)
  return data
}

export async function register(email: string, password: string): Promise<AuthResponse> {
  const res = await fetch('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({})) as { detail?: string; title?: string; errors?: Record<string, string[]> }
    const message = body.detail ?? body.title ?? (body.errors ? Object.values(body.errors).flat().join(' ') : null) ?? `Registration failed (${res.status})`
    throw new Error(message)
  }
  const data = (await res.json()) as AuthResponse
  setAuth(data.token, data.email)
  return data
}

export function logout(): void {
  clearAuth()
}
