# Coin Canvas – Mobile App (Capacitor)

The same React web app runs as a **native mobile app** on iOS and Android using [Capacitor](https://capacitorjs.com/). One codebase: web + mobile.

## Prerequisites

- **Node.js** and **npm** (already used for the frontend)
- **Android:** [Android Studio](https://developer.android.com/studio) and Android SDK
- **iOS (macOS only):** [Xcode](https://developer.apple.com/xcode/) and [CocoaPods](https://cocoapods.org/)  
  Install CocoaPods: `brew install cocoapods`

## How it works

- The frontend is built with **Vite** and the output (`dist/`) is embedded in the native projects.
- The mobile build uses **`VITE_API_URL=https://coincanvas.net`** so the app talks to your production API. No proxy: requests go directly to the server.
- **PWA (service worker)** is disabled for the mobile build to avoid build issues; the native app doesn’t need it.

## Build and run

**Important:** All commands below must be run from inside the `frontend` folder. From the repo root:

```bash
cd frontend
```

### 1. Build the web app for mobile

```bash
npm run build:mobile
```

This compiles the app and sets the API base to `https://coincanvas.net`. To use another API (e.g. staging), set `VITE_API_URL`:

```bash
VITE_API_URL=https://your-api.com npm run build:mobile
```

(Or change the `build:mobile` script in `package.json`.)

### 2. Sync to native projects

```bash
npm run cap:sync
```

This runs `build:mobile` and then `npx cap sync`, copying the built files into the Android (and iOS, if present) projects.

### 3. Run on device or emulator

**Android**

- Open the Android project: `npx cap open android` (or `npm run cap:android`).
- In Android Studio, pick an emulator or connected device and run the app.
- Or from the CLI: `npx cap run android` (with device/emulator available).

**iOS (macOS only)**

1. Install **CocoaPods** (required once):  
   `brew install cocoapods`

2. If you see *"ios platform has not been added yet"* or *"CocoaPods is not installed"*, add the iOS platform first (from the `frontend` folder):
   ```bash
   cd frontend
   npx cap add ios
   npx cap sync
   ```

3. Open the iOS project:  
   `npx cap open ios`  
   (or `npm run cap:ios` after the platform is added). In Xcode, select a simulator or device and run.

## Project layout (mobile)

- **`frontend/capacitor.config.ts`** – Capacitor config (app id, name, `webDir: dist`).
- **`frontend/android/`** – Android project (commit this if you want others to open it in Android Studio).
- **`frontend/ios/`** – iOS project (after `cap add ios`; commit if you use Xcode with the repo).
- **`frontend/src/config.ts`** – Defines `apiBaseUrl` from `VITE_API_URL` (empty = relative `/api` for web).

## Customization

- **App name / bundle ID:** Edit `frontend/capacitor.config.ts` (`appName`, `appId`). For iOS/Android package names, adjust the native projects (e.g. `android/app/build.gradle`, Xcode project settings).
- **API URL for mobile:** Change the URL in the `build:mobile` script or pass `VITE_API_URL` when building.
- **Icons and splash:** Replace assets in `android/app/src/main/res/` and `ios/App/App/Assets.xcassets/` (or use a Capacitor icon/splash plugin).

## Publishing

- **Android:** Build a release APK/AAB in Android Studio (e.g. Build → Generate Signed Bundle / APK) and upload to Google Play.
- **iOS:** In Xcode, set signing, then Archive and upload to App Store Connect.

For store listings, use the same descriptions and assets as the web app (see README and marketing copy).
