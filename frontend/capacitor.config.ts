import type { CapacitorConfig } from '@capacitor/cli'

const config: CapacitorConfig = {
  appId: 'net.coincanvas.app',
  appName: 'Coin Canvas',
  webDir: 'dist',
  server: {
    // In dev, optional: point to local API. For production build use VITE_API_URL instead.
    // androidScheme: 'https',
  },
}

export default config
