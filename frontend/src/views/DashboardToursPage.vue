<template>
  <div class="dashboard">
    <h1>My Tours</h1>
    <p class="hint">Authenticate with the API to list and manage your tours. Use the Howsee API (e.g. <code>GET /api/tours</code> with Bearer token).</p>
  </div>
</template>

<script setup>
</script>

<style scoped>
.dashboard {
  padding: 2rem;
  max-width: 600px;
  margin: 0 auto;
}
.hint {
  color: #666;
  font-size: 0.95rem;
}
code {
  background: #eee;
  padding: 0.2rem 0.4rem;
  border-radius: 4px;
}
</style>
