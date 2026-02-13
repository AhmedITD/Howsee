# Howsee Frontend (Vue 3)

Vue 3 + Vite SPA for secure Matterport virtual tours.

## Setup

```bash
npm install
```

## Config

Create `.env` (see `.env.example`):

- `VITE_API_BASE` – base URL of the Howsee API (e.g. `http://localhost:5000`). Leave empty if using Vite proxy.

## Run

```bash
npm run dev
```

With default Vite proxy, `/api` is forwarded to `http://localhost:5000`.

## Routes

- `/tour/:token` – Public tour viewer (Matterport embed + scene list). `token` is the share token from the API.
- `/dashboard/tours` – Placeholder for authenticated tour list (add login + Bearer token for full CRUD).

## Build

```bash
npm run build
```
