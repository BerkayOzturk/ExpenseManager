import { Navigate, useLocation } from 'react-router-dom'
import { getToken } from '../auth/storage'

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const location = useLocation()
  const token = getToken()

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <>{children}</>
}
