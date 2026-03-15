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

export async function loginWithGoogle(idToken: string): Promise<AuthResponse> {
  const res = await fetch('/api/auth/google', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken }),
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({})) as { detail?: string }
    throw new Error(body.detail ?? 'Google sign-in failed')
  }
  const data = (await res.json()) as AuthResponse
  setAuth(data.token, data.email)
  return data
}

export async function forgotPassword(email: string): Promise<void> {
  const res = await fetch('/api/auth/forgot-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({})) as { detail?: string }
    throw new Error(body.detail ?? 'Request failed')
  }
}

export async function resetPassword(email: string, code: string, newPassword: string): Promise<void> {
  const res = await fetch('/api/auth/reset-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, code, newPassword }),
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({})) as { detail?: string }
    throw new Error(body.detail ?? 'Reset failed')
  }
}

export function logout(): void {
  clearAuth()
}
