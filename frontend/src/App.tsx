import { Routes, Route, NavLink, useNavigate } from 'react-router-dom'
import { Footer } from './components/Footer'
import { ProtectedRoute } from './components/ProtectedRoute'
import { useAuth } from './contexts/AuthContext'
import { SettingsProvider } from './contexts/SettingsContext'
import { useTranslations } from './hooks/useTranslations'
import Budgets from './pages/Budgets'
import Categories from './pages/Categories'
import Expenses from './pages/Expenses'
import RecurringExpenses from './pages/RecurringExpenses'
import Login from './pages/Login'
import Register from './pages/Register'
import Settings from './pages/Settings'
import './App.css'

function LayoutWithNav() {
  const { email, logout } = useAuth()
  const navigate = useNavigate()
  const { t } = useTranslations()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="app">
      <nav className="nav">
        <div className="nav-links">
          <NavLink to="/" end className={({ isActive }) => (isActive ? 'active' : '')}>
            {t('nav_expenses')}
          </NavLink>
          <NavLink to="/categories" className={({ isActive }) => (isActive ? 'active' : '')}>
            {t('nav_categories')}
          </NavLink>
          <NavLink to="/budgets" className={({ isActive }) => (isActive ? 'active' : '')}>
            {t('nav_budgets')}
          </NavLink>
          <NavLink to="/recurring" className={({ isActive }) => (isActive ? 'active' : '')}>
            {t('nav_recurring')}
          </NavLink>
          <NavLink to="/settings" className={({ isActive }) => (isActive ? 'active' : '')}>
            {t('nav_settings')}
          </NavLink>
        </div>
        <div className="nav-user">
          <span className="nav-email" title={email ?? ''}>{email}</span>
          <button type="button" className="btn btn-secondary btn-sm" onClick={handleLogout}>
            {t('nav_logout')}
          </button>
        </div>
      </nav>
      <main className="main">
        <Routes>
          <Route path="/" element={<Expenses />} />
          <Route path="/categories" element={<Categories />} />
          <Route path="/budgets" element={<Budgets />} />
          <Route path="/recurring" element={<RecurringExpenses />} />
          <Route path="/settings" element={<Settings />} />
        </Routes>
      </main>
    </div>
  )
}

function App() {
  return (
    <SettingsProvider>
      <div className="site">
        <div className="site-content">
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="*" element={<ProtectedRoute><LayoutWithNav /></ProtectedRoute>} />
          </Routes>
        </div>
        <Footer />
      </div>
    </SettingsProvider>
  )
}

export default App
