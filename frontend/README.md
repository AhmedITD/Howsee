# Howsee Frontend (Vue 3)

Vue 3 + Vite SPA for secure Matterport virtual tours.

## Setup

```bash
npm install
```

## Config

Create `.env` (see `.env.example`):

- `VITE_API_BASE` – base URL of the Howsee API (e.g. `http://localhost:5000`). Leave empty if using Vite proxy.
- `VITE_MATTERPORT_SDK_VERSION` – (optional) Matterport SDK bootstrap version override. Default is set in code; only set if you need a specific version.

### Matterport SDK key (backend)

The 3D viewer receives the Matterport **application key** from the Howsee API when loading a tour (`GET /api/tour/view/{token}`). Configure the key on the **backend**:

1. Create an SDK key at [Matterport Cloud → Settings → Developer Tools → SDK Key Management](https://my.matterport.com/settings/account/devtools). Add your frontend origins to the key’s allow list (e.g. `localhost`, `localhost:5173`, and your production domain).
2. In the Howsee API, set `Matterport:ApplicationKey` in `appsettings.json` (or use environment variable `Matterport__ApplicationKey` in production so the key is not in source).
3. The API returns this key only when a tour view config is successfully validated (token, password, expiry). Never expose Matterport **API Token/Secret** (used for Model API) to the frontend.

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
