# Coin Canvas – Frontend

React + TypeScript + Vite. Uses the backend API at `http://localhost:5000` (proxied via Vite when using `npm run dev`).

## Setup

```bash
npm install
```

## Run (dev)

Start the backend first, then:

```bash
npm run dev
```

Open http://localhost:5173

## Build

```bash
npm run build
```

Output is in `dist/`. For production, serve the backend and point it to serve the static files from `dist`, or use a separate static host and set the API base URL accordingly.
