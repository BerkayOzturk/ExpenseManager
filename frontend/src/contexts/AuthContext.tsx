import { createContext, useCallback, useContext, useState } from 'react'
import { getStoredEmail, getToken } from '../auth/storage'
import * as authApi from '../api/auth'

interface AuthContextValue {
  email: string | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [email, setEmail] = useState<string | null>(() => getStoredEmail())
  const isAuthenticated = !!getToken() // re-read on each render so nav updates after login

  const login = useCallback(async (email: string, password: string) => {
    await authApi.login(email, password)
    setEmail(email)
  }, [])

  const register = useCallback(async (email: string, password: string) => {
    await authApi.register(email, password)
    setEmail(email)
  }, [])

  const logout = useCallback(() => {
    authApi.logout()
    setEmail(null)
  }, [])

  return (
    <AuthContext.Provider value={{ email, isAuthenticated, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
