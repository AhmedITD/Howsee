<template>
  <div class="dashboard">
    <h1>Properties</h1>

    <section v-if="!isAuthenticated()" class="auth-section">
      <p class="hint">Set your Howsee API token to list and manage properties.</p>
      <div class="token-form">
        <input
          v-model="tokenInput"
          type="password"
          placeholder="Paste Bearer token"
          class="token-input"
        />
        <button type="button" class="btn btn-primary" @click="saveToken">Save token</button>
      </div>
    </section>

    <template v-else>
      <div class="nav-links">
        <router-link to="/dashboard/tours" class="nav-link">Tours</router-link>
        <router-link to="/dashboard/properties" class="nav-link active">Properties</router-link>
      </div>
      <div class="toolbar">
        <button type="button" class="btn btn-secondary" @click="clearToken">Clear token</button>
        <button type="button" class="btn btn-primary" @click="openCreate">New property</button>
      </div>

      <div v-if="loading" class="loading">Loading properties…</div>
      <div v-else-if="listError" class="error">{{ listError }}</div>
      <div v-else-if="properties.length === 0" class="empty">No properties yet. Create one above.</div>
      <div v-else class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>Category</th>
              <th>Address / Locality</th>
              <th>Area</th>
              <th>Active</th>
              <th>Tour</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in properties" :key="p.id">
              <td>{{ categoryLabel(p.category) }}</td>
              <td>{{ p.address || p.locality || '—' }}</td>
              <td>{{ p.area != null ? p.area : '—' }}</td>
              <td>{{ p.active ? 'Yes' : 'No' }}</td>
              <td>{{ p.tourTitle || '—' }}</td>
              <td>
                <button type="button" class="btn btn-small" @click="openEdit(p)">Edit</button>
                <button type="button" class="btn btn-small btn-danger" @click="confirmDelete(p)">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <section v-if="showForm" class="form-section">
        <h2>{{ editingId ? 'Edit property' : 'New property' }}</h2>
        <form class="property-form" @submit.prevent="submitForm">
          <div class="form-row">
            <label for="category">Category *</label>
            <select id="category" v-model.number="form.category" required>
              <option :value="0">Department</option>
              <option :value="1">House</option>
            </select>
          </div>
          <div class="form-row">
            <label for="address">Address</label>
            <input id="address" v-model="form.address" type="text" />
          </div>
          <div class="form-row">
            <label for="locality">Locality</label>
            <input id="locality" v-model="form.locality" type="text" />
          </div>
          <div class="form-row">
            <label for="administrativeArea">Administrative area</label>
            <input id="administrativeArea" v-model="form.administrativeArea" type="text" />
          </div>
          <div class="form-row">
            <label for="countryCode">Country code</label>
            <input id="countryCode" v-model="form.countryCode" type="text" maxlength="10" />
          </div>
          <div class="form-row">
            <label for="postalCode">Postal code</label>
            <input id="postalCode" v-model="form.postalCode" type="text" />
          </div>
          <div class="form-row">
            <label for="lat">Latitude</label>
            <input id="lat" v-model.number="form.lat" type="number" step="any" placeholder="-90 to 90" />
          </div>
          <div class="form-row">
            <label for="lng">Longitude</label>
            <input id="lng" v-model.number="form.lng" type="number" step="any" placeholder="-180 to 180" />
          </div>
          <div class="form-row">
            <label for="description">Description</label>
            <textarea id="description" v-model="form.description" rows="3"></textarea>
          </div>
          <div class="form-row">
            <label for="area">Area</label>
            <input id="area" v-model.number="form.area" type="number" step="0.01" min="0" placeholder="e.g. sq m" />
          </div>
          <div class="form-row">
            <label for="tourId">Tour</label>
            <select id="tourId" v-model.number="form.tourId" class="select-full">
              <option :value="null">— No tour —</option>
              <option v-for="t in tours" :key="t.id" :value="t.id">{{ t.title }}</option>
            </select>
          </div>
          <div class="form-row">
            <label>
              <input v-model="form.active" type="checkbox" />
              Active
            </label>
          </div>
          <div class="form-actions">
            <button type="button" class="btn btn-secondary" @click="closeForm">Cancel</button>
            <button type="submit" class="btn btn-primary" :disabled="formSaving">
              {{ formSaving ? 'Saving…' : (editingId ? 'Update' : 'Create') }}
            </button>
          </div>
          <div v-if="formError" class="error">{{ formError }}</div>
        </form>
      </section>

      <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
        <div class="modal">
          <p>Delete property "{{ deleteTarget.address || deleteTarget.locality || deleteTarget.id }}"?</p>
          <div class="modal-actions">
            <button type="button" class="btn btn-secondary" @click="deleteTarget = null">Cancel</button>
            <button type="button" class="btn btn-danger" :disabled="deleting" @click="doDelete">
              {{ deleting ? 'Deleting…' : 'Delete' }}
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useApiAuth } from '../composables/useApiAuth'

const apiBase = import.meta.env.VITE_API_BASE || ''
const { token, setToken, clearToken, authHeaders, isAuthenticated } = useApiAuth()

const tokenInput = ref('')
const properties = ref([])
const tours = ref([])
const loading = ref(false)
const listError = ref(null)
const showForm = ref(false)
const editingId = ref(null)
const form = ref({
  category: 0,
  address: '',
  locality: '',
  administrativeArea: '',
  countryCode: '',
  postalCode: '',
  lat: null,
  lng: null,
  description: '',
  area: null,
  tourId: null,
  active: true
})
const formSaving = ref(false)
const formError = ref(null)
const deleteTarget = ref(null)
const deleting = ref(false)

function categoryLabel(category) {
  return category === 1 ? 'House' : 'Department'
}

function saveToken() {
  setToken(tokenInput.value)
  tokenInput.value = ''
}

async function fetchTours() {
  try {
    const res = await fetch(`${apiBase}/api/tours`, {
      headers: { 'Content-Type': 'application/json', ...authHeaders() }
    })
    const data = await res.json()
    if (data.success && data.data && Array.isArray(data.data)) {
      tours.value = data.data.map((t) => ({
        id: t.id ?? t.Id,
        title: t.title ?? t.Title
      }))
    } else {
      tours.value = []
    }
  } catch (_) {
    tours.value = []
  }
}

async function fetchProperties() {
  loading.value = true
  listError.value = null
  try {
    const res = await fetch(`${apiBase}/api/properties`, {
      headers: authHeaders()
    })
    const data = await res.json()
    if (!data.success) {
      listError.value = data.message || 'Failed to load properties'
      if (res.status === 401) clearToken()
      properties.value = []
      return
    }
    const raw = data.data || []
    properties.value = raw.map((p) => ({
      id: p.id ?? p.Id,
      category: p.category ?? p.Category,
      lat: p.lat ?? p.Lat,
      lng: p.lng ?? p.Lng,
      description: p.description ?? p.Description,
      area: p.area ?? p.Area,
      active: p.active ?? p.Active,
      tourId: p.tourId ?? p.TourId,
      tourTitle: p.tourTitle ?? p.TourTitle,
      address: p.address ?? p.Address,
      locality: p.locality ?? p.Locality,
      administrativeArea: p.administrativeArea ?? p.AdministrativeArea,
      countryCode: p.countryCode ?? p.CountryCode,
      postalCode: p.postalCode ?? p.PostalCode,
      createdAt: p.createdAt ?? p.CreatedAt
    }))
  } catch (e) {
    listError.value = 'Network error. Please try again.'
    properties.value = []
  } finally {
    loading.value = false
  }
}

watch(isAuthenticated, (auth) => {
  if (auth) {
    fetchProperties()
    fetchTours()
  } else {
    properties.value = []
    tours.value = []
  }
}, { immediate: true })

function openCreate() {
  editingId.value = null
  form.value = {
    category: 0,
    address: '',
    locality: '',
    administrativeArea: '',
    countryCode: '',
    postalCode: '',
    lat: null,
    lng: null,
    description: '',
    area: null,
    tourId: null,
    active: true
  }
  formError.value = null
  showForm.value = true
}

function openEdit(p) {
  editingId.value = p.id
  form.value = {
    category: p.category,
    address: p.address ?? '',
    locality: p.locality ?? '',
    administrativeArea: p.administrativeArea ?? '',
    countryCode: p.countryCode ?? '',
    postalCode: p.postalCode ?? '',
    lat: p.lat,
    lng: p.lng,
    description: p.description ?? '',
    area: p.area,
    tourId: p.tourId,
    active: p.active
  }
  formError.value = null
  showForm.value = true
}

function closeForm() {
  showForm.value = false
  editingId.value = null
}

async function submitForm() {
  formSaving.value = true
  formError.value = null
  try {
    const payload = {
      category: form.value.category,
      address: form.value.address || null,
      locality: form.value.locality || null,
      administrativeArea: form.value.administrativeArea || null,
      countryCode: form.value.countryCode || null,
      postalCode: form.value.postalCode || null,
      lat: form.value.lat,
      lng: form.value.lng,
      description: form.value.description || null,
      area: form.value.area,
      tourId: form.value.tourId ?? null,
      active: form.value.active
    }
    if (editingId.value && !form.value.tourId) payload.clearTourId = true
    const url = editingId.value ? `${apiBase}/api/properties/${editingId.value}` : `${apiBase}/api/properties`
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
    await fetchProperties()
    closeForm()
  } catch (e) {
    formError.value = 'Network error. Please try again.'
  } finally {
    formSaving.value = false
  }
}

function confirmDelete(p) {
  deleteTarget.value = p
  deleting.value = false
}

async function doDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    const res = await fetch(`${apiBase}/api/properties/${deleteTarget.value.id}`, {
      method: 'DELETE',
      headers: authHeaders()
    })
    const data = await res.json()
    if (!data.success && res.status !== 404) {
      listError.value = data.message || 'Delete failed'
      if (res.status === 401) clearToken()
    } else {
      await fetchProperties()
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
.table-wrap {
  overflow-x: auto;
}
.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}
.data-table th,
.data-table td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}
.data-table th {
  font-weight: 600;
  background: #f9fafb;
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
.form-row input[type='number'],
.form-row select,
.form-row textarea {
  width: 100%;
  max-width: 400px;
  padding: 0.5rem 0.75rem;
  font-size: 1rem;
}
.select-full {
  max-width: 400px;
}
.form-actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
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
  margin-top: 1rem;
  justify-content: flex-end;
}
</style>
