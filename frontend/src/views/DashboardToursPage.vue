<template>
  <div class="dashboard">
    <h1>My Tours</h1>

    <section v-if="!isAuthenticated()" class="auth-section">
      <p class="hint">Set your Howsee API token to list and manage tours. Get a token by logging in via the API (e.g. login endpoint).</p>
      <div class="token-form">
        <input
          v-model="tokenInput"
          type="password"
          placeholder="Paste Bearer token"
          class="token-input"
        />
        <button
          type="button"
          class="btn btn-primary"
          @click="saveToken"
        >
          Save token
        </button>
      </div>
    </section>

    <template v-else>
      <div class="nav-links">
        <router-link to="/dashboard/tours" class="nav-link active">Tours</router-link>
        <router-link to="/dashboard/properties" class="nav-link">Properties</router-link>
      </div>
      <div class="toolbar">
        <button
          type="button"
          class="btn btn-secondary"
          @click="clearToken"
        >
          Clear token
        </button>
        <button
          type="button"
          class="btn btn-primary"
          @click="openCreate"
        >
          New tour
        </button>
      </div>

      <div v-if="loading" class="loading">Loading tours…</div>
      <div v-else-if="listError" class="error">{{ listError }}</div>
      <div v-else-if="tours.length === 0" class="empty">No tours yet. Create one above.</div>
      <div v-else class="tour-list">
        <table class="tour-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Model ID</th>
              <th>Expires</th>
              <th>Protected</th>
              <th>Share link</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="t in tours"
              :key="t.id"
            >
              <td>{{ t.title }}</td>
              <td class="mono">{{ t.matterportModelId }}</td>
              <td>{{ formatExpiry(t.expiresAt) }}</td>
              <td>{{ t.hasPassword ? 'Yes' : 'No' }}</td>
              <td>
                <span class="share-link">{{ shareLink(t.shareToken) }}</span>
                <button
                  type="button"
                  class="btn btn-small"
                  @click="copyShareLink(t.shareToken)"
                >
                  Copy
                </button>
              </td>
              <td>
                <button
                  type="button"
                  class="btn btn-small"
                  @click="openEdit(t)"
                >
                  Edit
                </button>
                <button
                  type="button"
                  class="btn btn-small btn-danger"
                  @click="confirmDelete(t)"
                >
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <section v-if="showForm" class="form-section">
        <h2>{{ editingId ? 'Edit tour' : 'New tour' }}</h2>
        <form
          class="tour-form"
          @submit.prevent="submitForm"
        >
          <div class="form-row">
            <label for="title">Title *</label>
            <input
              id="title"
              v-model="form.title"
              type="text"
              required
            />
          </div>
          <div class="form-row">
            <label for="matterportModelId">Matterport model ID *</label>
            <div class="model-id-row">
              <input
                id="matterportModelId"
                v-model="form.matterportModelId"
                type="text"
                required
              />
              <input
                v-model="modelSearchQuery"
                type="text"
                placeholder="Search (e.g. name: Demo)"
                class="model-search-input"
              />
              <button
                type="button"
                class="btn btn-secondary"
                :disabled="loadingModels"
                @click="loadMatterportModels"
              >
                {{ loadingModels ? 'Loading…' : 'Pick from Matterport' }}
              </button>
            </div>
            <select
              v-if="matterportModels.length"
              v-model="form.matterportModelId"
              class="model-select"
            >
              <option value="">-- Select a model --</option>
              <option
                v-for="m in matterportModels"
                :key="m.id"
                :value="m.id"
              >
                {{ modelOptionLabel(m) }}
              </option>
            </select>
            <button
              v-if="modelsNextOffset"
              type="button"
              class="btn btn-small"
              :disabled="loadingModels"
              @click="loadMoreMatterportModels"
            >
              {{ loadingModels ? 'Loading…' : 'Load more' }}
            </button>
          </div>
          <div class="form-row">
            <label for="startSweepId">Start sweep ID (optional)</label>
            <div class="sweep-row">
              <select
                v-if="sweepLocations.length"
                v-model="form.startSweepId"
                class="sweep-select"
              >
                <option value="">-- No start sweep --</option>
                <option
                  v-for="loc in sweepLocations"
                  :key="loc.id"
                  :value="loc.id"
                >
                  {{ loc.label || loc.id }}{{ loc.roomId ? ` (${loc.roomId})` : '' }}
                </option>
              </select>
              <input
                v-else
                id="startSweepId"
                v-model="form.startSweepId"
                type="text"
                placeholder="Sweep/location ID or pick model first"
              />
              <span v-if="loadingLocations" class="hint">Loading locations…</span>
            </div>
          </div>
          <div class="form-row">
            <label for="password">Password (optional)</label>
            <input
              id="password"
              v-model="form.password"
              type="password"
              :placeholder="editingId && tourHasPassword ? 'Leave blank to keep' : ''"
            />
          </div>
          <div class="form-row">
            <label for="expiresAt">Expires at (optional)</label>
            <input
              id="expiresAt"
              v-model="form.expiresAt"
              type="datetime-local"
            />
          </div>
          <div v-if="editingId" class="form-row">
            <label>
              <input
                v-model="form.isActive"
                type="checkbox"
              />
              Active
            </label>
          </div>
          <div class="form-actions">
            <button
              type="button"
              class="btn btn-secondary"
              @click="closeForm"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="formSaving"
            >
              {{ formSaving ? 'Saving…' : (editingId ? 'Update' : 'Create') }}
            </button>
          </div>
          <div v-if="formError" class="error">{{ formError }}</div>
        </form>
        <div v-if="createdShareToken" class="created-share">
          <p>Tour created. Share link:</p>
          <div class="share-row">
            <input
              :value="shareLink(createdShareToken)"
              readonly
              class="share-input"
            />
            <button
              type="button"
              class="btn btn-small"
              @click="copyShareLink(createdShareToken)"
            >
              Copy
            </button>
          </div>
        </div>
      </section>

      <div
        v-if="deleteTarget"
        class="modal-overlay"
        @click.self="deleteTarget = null"
      >
        <div class="modal">
          <p>Delete tour “{{ deleteTarget.title }}”?</p>
          <div class="modal-actions">
            <button
              type="button"
              class="btn btn-secondary"
              @click="deleteTarget = null"
            >
              Cancel
            </button>
            <button
              type="button"
              class="btn btn-danger"
              :disabled="deleting"
              @click="doDelete"
            >
              {{ deleting ? 'Deleting…' : 'Delete' }}
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useApiAuth } from '../composables/useApiAuth'

const apiBase = import.meta.env.VITE_API_BASE || ''
const { token, setToken, clearToken, authHeaders, isAuthenticated } = useApiAuth()

const tokenInput = ref('')
const tours = ref([])
const loading = ref(false)
const listError = ref(null)
const showForm = ref(false)
const editingId = ref(null)
const form = ref({
  title: '',
  matterportModelId: '',
  startSweepId: '',
  password: '',
  expiresAt: '',
  isActive: true
})
const formSaving = ref(false)
const formError = ref(null)
const createdShareToken = ref(null)
const deleteTarget = ref(null)
const deleting = ref(false)
const matterportModels = ref([])
const modelsNextOffset = ref(null)
const loadingModels = ref(false)
const sweepLocations = ref([])
const loadingLocations = ref(false)
const modelSearchQuery = ref('')

const tourHasPassword = computed(() => {
  if (!editingId.value) return false
  const t = tours.value.find((x) => x.id === editingId.value)
  return t?.hasPassword ?? false
})

function saveToken() {
  setToken(tokenInput.value)
  tokenInput.value = ''
}

function shareLink(shareToken) {
  if (!shareToken) return ''
  const origin = typeof window !== 'undefined' ? window.location.origin : ''
  return `${origin}/tour/${shareToken}`
}

function formatExpiresAt(expiresAt) {
  if (!expiresAt) return '—'
  const d = new Date(expiresAt)
  return Number.isNaN(d.getTime()) ? expiresAt : d.toLocaleString()
}

const formatExpiry = formatExpiresAt

async function copyShareLink(shareToken) {
  const url = shareLink(shareToken)
  if (!url) return
  try {
    await navigator.clipboard.writeText(url)
  } catch (_) {
    // fallback
    const el = document.createElement('input')
    el.value = url
    document.body.appendChild(el)
    el.select()
    document.execCommand('copy')
    document.body.removeChild(el)
  }
}

async function fetchTours() {
  loading.value = true
  listError.value = null
  try {
    const res = await fetch(`${apiBase}/api/tours`, {
      headers: { 'Content-Type': 'application/json', ...authHeaders() }
    })
    const data = await res.json()
    if (!data.success) {
      listError.value = data.message || 'Failed to load tours'
      if (res.status === 401) clearToken()
      tours.value = []
      return
    }
    const raw = data.data || []
    tours.value = raw.map((t) => ({
      id: t.id ?? t.Id,
      title: t.title ?? t.Title,
      matterportModelId: t.matterportModelId ?? t.MatterportModelId,
      startSweepId: t.startSweepId ?? t.StartSweepId,
      hasPassword: t.hasPassword ?? t.HasPassword,
      expiresAt: t.expiresAt ?? t.ExpiresAt,
      isActive: t.isActive ?? t.IsActive,
      shareToken: t.shareToken ?? t.ShareToken
    }))
  } catch (e) {
    listError.value = 'Network error. Please try again.'
    tours.value = []
  } finally {
    loading.value = false
  }
}

watch(isAuthenticated, (auth) => {
  if (auth) fetchTours()
  else tours.value = []
}, { immediate: true })

function openCreate() {
  editingId.value = null
  createdShareToken.value = null
  form.value = {
    title: '',
    matterportModelId: '',
    startSweepId: '',
    password: '',
    expiresAt: '',
    isActive: true
  }
  formError.value = null
  showForm.value = true
}

function openEdit(t) {
  editingId.value = t.id
  createdShareToken.value = null
  const expiresAt = t.expiresAt ? new Date(t.expiresAt).toISOString().slice(0, 16) : ''
  form.value = {
    title: t.title,
    matterportModelId: t.matterportModelId,
    startSweepId: t.startSweepId || '',
    password: '',
    expiresAt,
    isActive: t.isActive
  }
  formError.value = null
  showForm.value = true
}

function closeForm() {
  showForm.value = false
  editingId.value = null
  createdShareToken.value = null
}

async function submitForm() {
  formSaving.value = true
  formError.value = null
  try {
    const payload = {
      title: form.value.title,
      matterportModelId: form.value.matterportModelId,
      startSweepId: form.value.startSweepId || null,
      password: form.value.password || null,
      expiresAt: form.value.expiresAt ? new Date(form.value.expiresAt).toISOString() : null
    }
    if (editingId.value) {
      payload.isActive = form.value.isActive
    }
    const url = editingId.value
      ? `${apiBase}/api/tours/${editingId.value}`
      : `${apiBase}/api/tours`
    const method = editingId.value ? 'PUT' : 'POST'
    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json', ...authHeaders() },
      body: JSON.stringify(payload)
    })
    const data = await res.json()
    if (!data.success) {
      formError.value = data.message || (data.errors ? JSON.stringify(data.errors) : 'Request failed')
      if (res.status === 401) clearToken()
      return
    }
    const d = data.data
    const token = d?.shareToken ?? d?.ShareToken
    if (token) createdShareToken.value = token
    await fetchTours()
    if (!editingId.value) {
      createdShareToken.value = token || null
    } else {
      closeForm()
    }
  } catch (e) {
    formError.value = 'Network error. Please try again.'
  } finally {
    formSaving.value = false
  }
}

function confirmDelete(t) {
  deleteTarget.value = t
  deleting.value = false
}

async function loadMatterportModels(append = false) {
  loadingModels.value = true
  if (!append) {
    matterportModels.value = []
    modelsNextOffset.value = null
  }
  try {
    const params = new URLSearchParams({ pageSize: '50' })
    if (modelSearchQuery.value) params.set('query', modelSearchQuery.value)
    if (append && modelsNextOffset.value) params.set('offset', modelsNextOffset.value)
    const res = await fetch(`${apiBase}/api/tours/matterport-models?${params}`, {
      headers: authHeaders()
    })
    const data = await res.json()
    if (!data.success) {
      if (!append) listError.value = data.message || 'Could not load Matterport models.'
      return
    }
    const payload = data.data?.results ?? data.data
    const nextOffset = data.data?.nextOffset ?? data.data?.NextOffset ?? null
    const list = Array.isArray(payload) ? payload : (payload?.Results ?? [])
    const mapped = list.map((m) => ({
      id: m.id ?? m.Id,
      name: m.name ?? m.Name,
      description: m.description ?? m.Description,
      visibility: m.visibility ?? m.Visibility,
      created: m.created ?? m.Created,
      modified: m.modified ?? m.Modified
    }))
    if (append) matterportModels.value = [...matterportModels.value, ...mapped]
    else matterportModels.value = mapped
    modelsNextOffset.value = nextOffset
  } catch (_) {
    if (!append) listError.value = 'Could not load Matterport models.'
  } finally {
    loadingModels.value = false
  }
}

async function loadMoreMatterportModels() {
  await loadMatterportModels(true)
}

async function fetchSweepLocations(modelId) {
  if (!modelId) {
    sweepLocations.value = []
    return
  }
  loadingLocations.value = true
  sweepLocations.value = []
  try {
    const res = await fetch(`${apiBase}/api/tours/matterport-models/${encodeURIComponent(modelId)}/locations`, {
      headers: authHeaders()
    })
    const data = await res.json()
    if (data.success && data.data && Array.isArray(data.data)) {
      sweepLocations.value = data.data.map((loc) => ({
        id: loc.id ?? loc.Id,
        label: loc.label ?? loc.Label,
        roomId: loc.roomId ?? loc.RoomId,
        floorId: loc.floorId ?? loc.FloorId
      }))
    }
  } catch (_) {
    sweepLocations.value = []
  } finally {
    loadingLocations.value = false
  }
}

function modelOptionLabel(m) {
  const name = m.name || m.id
  if (m.description) return `${name} – ${m.description.length > 40 ? m.description.slice(0, 40) + '…' : m.description}`
  if (m.modified) return `${name} (${new Date(m.modified).toLocaleDateString()})`
  return name
}

watch(
  () => form.value.matterportModelId,
  (modelId, prev) => {
    sweepLocations.value = []
    if (prev && modelId !== prev) form.value.startSweepId = ''
    if (modelId) fetchSweepLocations(modelId)
  }
)

async function doDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    const res = await fetch(`${apiBase}/api/tours/${deleteTarget.value.id}`, {
      method: 'DELETE',
      headers: authHeaders()
    })
    const data = await res.json()
    if (!data.success && res.status !== 404) {
      listError.value = data.message || 'Delete failed'
      if (res.status === 401) clearToken()
    } else {
      await fetchTours()
      deleteTarget.value = null
    }
  } catch (e) {
    listError.value = 'Network error.'
  } finally {
    deleting.value = false
  }
}
</script>

<style scoped>
.dashboard {
  padding: 2rem;
  max-width: 960px;
  margin: 0 auto;
}
.hint {
  color: #666;
  font-size: 0.95rem;
  margin-bottom: 1rem;
}
.auth-section {
  margin-bottom: 2rem;
}
.token-form {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
.token-input {
  flex: 1;
  max-width: 400px;
  padding: 0.5rem 0.75rem;
  font-size: 1rem;
}
.nav-links {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}
.nav-link {
  color: #4b5563;
  text-decoration: none;
}
.nav-link.active {
  font-weight: 600;
  color: #2563eb;
}
.toolbar {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1.5rem;
}
.btn {
  padding: 0.5rem 1rem;
  font-size: 0.95rem;
  cursor: pointer;
  border: 1px solid #ccc;
  border-radius: 6px;
  background: #fff;
}
.btn-primary {
  background: #2563eb;
  color: #fff;
  border-color: #2563eb;
}
.btn-secondary {
  background: #f3f4f6;
}
.btn-danger {
  background: #dc2626;
  color: #fff;
  border-color: #dc2626;
}
.btn-small {
  padding: 0.25rem 0.5rem;
  font-size: 0.85rem;
  margin-left: 0.25rem;
}
.loading,
.error,
.empty {
  padding: 1rem;
  text-align: center;
}
.error {
  color: #b91c1c;
}
.tour-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}
.tour-table th,
.tour-table td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}
.tour-table th {
  font-weight: 600;
  background: #f9fafb;
}
.mono {
  font-family: ui-monospace, monospace;
  font-size: 0.85rem;
}
.share-link {
  font-size: 0.85rem;
  color: #4b5563;
  margin-right: 0.5rem;
}
.form-section {
  margin-top: 2rem;
  padding: 1.5rem;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: #f9fafb;
}
.form-section h2 {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}
.form-row {
  margin-bottom: 1rem;
}
.form-row label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
  font-size: 0.9rem;
}
.form-row input[type='text'],
.form-row input[type='password'],
.form-row input[type='datetime-local'] {
  width: 100%;
  max-width: 400px;
  padding: 0.5rem 0.75rem;
  font-size: 1rem;
}
.model-id-row {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}
.model-id-row input {
  max-width: 280px;
}
.model-search-input {
  max-width: 220px;
  padding: 0.35rem 0.5rem;
}
.model-select,
.sweep-select {
  margin-top: 0.5rem;
  width: 100%;
  max-width: 400px;
}
.sweep-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
}
.sweep-row input {
  flex: 1;
  min-width: 200px;
}
.model-select {
  width: 100%;
  max-width: 400px;
  padding: 0.5rem 0.75rem;
  font-size: 1rem;
}
.form-actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
}
.created-share {
  margin-top: 1rem;
  padding: 1rem;
  background: #ecfdf5;
  border-radius: 6px;
}
.share-row {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-top: 0.5rem;
}
.share-input {
  flex: 1;
  padding: 0.5rem;
  font-size: 0.9rem;
}
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
}
.modal {
  background: #fff;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 320px;
}
.modal-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  margin-top: 1rem;
}
</style>
