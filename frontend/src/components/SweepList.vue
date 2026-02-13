<template>
  <div class="sweep-list">
    <h4>Scenes</h4>
    <ul v-if="sweeps.length">
      <li
        v-for="s in sweeps"
        :key="s.id"
        :class="{ active: s.id === currentSweepId }"
        @click="onSelect(s.id)"
      >
        {{ s.sid || s.id || 'Scene' }}
      </li>
    </ul>
    <p v-else class="hint">No scenes yet</p>
  </div>
</template>

<script setup>
defineProps({
  sweeps: { type: Array, default: () => [] },
  currentSweepId: { type: String, default: '' }
})
const emit = defineEmits(['moveTo'])
function onSelect(sweepId) {
  emit('moveTo', sweepId)
}
</script>

<style scoped>
.sweep-list {
  position: fixed;
  top: 1rem;
  left: 1rem;
  z-index: 10;
  background: rgba(255, 255, 255, 0.95);
  padding: 0.75rem 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  max-height: 50vh;
  overflow-y: auto;
}
.sweep-list h4 {
  margin: 0 0 0.5rem;
  font-size: 0.9rem;
}
.sweep-list ul {
  list-style: none;
  margin: 0;
  padding: 0;
}
.sweep-list li {
  padding: 0.35rem 0.5rem;
  cursor: pointer;
  border-radius: 4px;
  font-size: 0.85rem;
}
.sweep-list li:hover {
  background: #eee;
}
.sweep-list li.active {
  background: #333;
  color: #fff;
}
.hint {
  margin: 0;
  font-size: 0.85rem;
  color: #666;
}
</style>
