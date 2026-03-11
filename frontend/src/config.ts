/**
 * API base URL for requests.
 * - Web (default): '' → relative /api (same origin).
 * - Mobile (Capacitor): set VITE_API_URL to e.g. https://coincanvas.net so the app talks to your backend.
 */
const origin = (import.meta.env.VITE_API_URL ?? '').toString().trim()
export const apiBaseUrl = origin ? `${origin.replace(/\/$/, '')}/api` : '/api'
