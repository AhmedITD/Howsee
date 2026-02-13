import { ref, watch } from 'vue'

const STORAGE_KEY = 'howsee_api_token'

const token = ref(typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null)

watch(token, (val) => {
  if (typeof localStorage === 'undefined') return
  if (val) localStorage.setItem(STORAGE_KEY, val)
  else localStorage.removeItem(STORAGE_KEY)
}, { immediate: false })

export function useApiAuth() {
  function setToken(newToken) {
    token.value = newToken ? String(newToken).trim() || null : null
  }

  function clearToken() {
    token.value = null
  }

  function authHeaders() {
    if (!token.value) return {}
    return { Authorization: `Bearer ${token.value}` }
  }

  return {
    token,
    setToken,
    clearToken,
    authHeaders,
    isAuthenticated: () => !!token.value
  }
}
