import { createContext, useContext, useEffect, useState } from 'react'
import { GoogleOAuthProvider } from '@react-oauth/google'
import { apiBaseUrl } from '../config'

interface AuthConfigState {
  googleClientId: string
}

const AuthConfigContext = createContext<AuthConfigState>({ googleClientId: '' })

export function AuthConfigProvider({ children }: { children: React.ReactNode }) {
  const [googleClientId, setGoogleClientId] = useState('')

  useEffect(() => {
    fetch(`${apiBaseUrl}/auth/config`)
      .then((res) => (res.ok ? res.json() : {}))
      .then((data: { googleClientId?: string }) => setGoogleClientId(data.googleClientId ?? ''))
      .catch(() => {})
  }, [])

  const value = { googleClientId }

  return (
    <AuthConfigContext.Provider value={value}>
      <GoogleOAuthProvider clientId={googleClientId}>
        {children}
      </GoogleOAuthProvider>
    </AuthConfigContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components -- context + hook in one file
export function useAuthConfig() {
  return useContext(AuthConfigContext)
}
