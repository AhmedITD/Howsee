<template>
  <div class="tour-page">
    <div v-if="loading" class="loading">Loading tour…</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="needsPassword" class="password-gate">
      <form @submit.prevent="submitPassword">
        <label>This tour is protected. Enter password:</label>
        <input v-model="password" type="password" placeholder="Password" required />
        <button type="submit">Continue</button>
      </form>
    </div>
    <div v-else-if="config && isExpired" class="error">This tour link has expired.</div>
    <template v-else-if="config">
      <TourViewer
        :matterport-model-id="config.matterportModelId"
        :application-key="config.applicationKey"
        :start-sweep-id="config.startSweepId"
      />
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import TourViewer from '../components/TourViewer.vue'

const props = defineProps({ token: { type: String, required: true } })
const route = useRoute()
const token = ref(route.params.token)
const loading = ref(true)
const error = ref(null)
const needsPassword = ref(false)
const password = ref('')
const config = ref(null)

const isExpired = computed(() => {
  const at = config.value?.expiresAt
  if (!at) return false
  return new Date(at) < new Date()
})

const apiBase = import.meta.env.VITE_API_BASE || ''

async function fetchConfig(pwd = null) {
  loading.value = true
  error.value = null
  needsPassword.value = false
  try {
    const url = `${apiBase}/api/tour/view/${encodeURIComponent(token.value)}`
    const options = pwd != null ? { method: 'GET' } : {}
    const res = await fetch(pwd != null ? `${url}?password=${encodeURIComponent(pwd)}` : url, options)
    const data = await res.json()
    if (!data.success) {
      if (data.code === 'TOUR_ACCESS_DENIED' && !pwd) {
        needsPassword.value = true
        return
      }
      error.value = data.message || 'Could not load tour.'
      return
    }
    config.value = data.data
  } catch (e) {
    error.value = 'Network error. Please try again.'
  } finally {
    loading.value = false
  }
}

function submitPassword() {
  fetchConfig(password.value)
}

watch(() => route.params.token, (newToken) => {
  token.value = newToken
  config.value = null
  fetchConfig()
}, { immediate: false })

onMounted(() => {
  fetchConfig()
})
</script>

<style scoped>
.tour-page {
  position: relative;
  width: 100%;
  height: 100vh;
}
.loading, .error {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  font-size: 1.2rem;
}
.error { color: #c00; }
.password-gate {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  background: #1a1a1a;
  color: #eee;
}
.password-gate form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 1.5rem;
}
.password-gate input {
  padding: 0.5rem 0.75rem;
  font-size: 1rem;
}
.password-gate button {
  padding: 0.5rem 1rem;
  cursor: pointer;
}
</style>
