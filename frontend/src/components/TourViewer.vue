<template>
  <div class="tour-viewer">
    <div
      v-if="!ready && sdkError"
      class="sdk-error"
    >
      Could not connect to 3D viewer. Check your link or try again.
      <p class="sdk-error-detail">{{ sdkError }}</p>
    </div>
    <iframe
      ref="iframeRef"
      :src="iframeSrc"
      class="showcase-iframe"
      allow="fullscreen"
      allowfullscreen
      @load="onIframeLoad"
    />
    <SweepList
      v-if="ready"
      :sweeps="sweeps"
      :current-sweep-id="currentSweep.id"
      @move-to="moveTo"
    />
    <div
      v-if="ready && sweeps.length > 1"
      class="guided-tour"
    >
      <span class="guided-tour-label">Guided tour</span>
      <select
        v-model="guidedEndSweepId"
        class="guided-tour-select"
      >
        <option
          v-for="s in sweeps"
          :key="s.id"
          :value="s.id"
          :disabled="s.id === currentSweep.id"
        >
          {{ s.sid || s.id || 'Scene' }}
        </option>
      </select>
      <button
        type="button"
        class="guided-tour-btn"
        :disabled="guidedPlaying || !guidedEndSweepId || guidedEndSweepId === currentSweep.id"
        @click="startGuidedTour"
      >
        {{ guidedPlaying ? 'Playing…' : 'Start' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useMatterportSdk } from '../composables/useMatterportSdk'
import SweepList from './SweepList.vue'

const props = defineProps({
  matterportModelId: { type: String, required: true },
  applicationKey: { type: String, required: true },
  startSweepId: { type: String, default: null }
})

const iframeRef = ref(null)
const iframeSrc = computed(() => {
  const m = encodeURIComponent(props.matterportModelId)
  const key = encodeURIComponent(props.applicationKey)
  return `https://my.matterport.com/show?m=${m}&play=1&applicationKey=${key}`
})

const {
  loadSdk,
  currentSweep,
  sweeps,
  moveTo,
  findPath,
  playPath,
  ready,
  error: sdkError
} = useMatterportSdk(iframeRef, props.applicationKey)

const guidedEndSweepId = ref('')
const guidedPlaying = ref(false)

async function onIframeLoad() {
  await loadSdk()
}

watch(ready, (isReady) => {
  if (isReady && props.startSweepId) {
    moveTo(props.startSweepId).catch(() => {})
  }
})

watch(sweeps, (list) => {
  if (list.length && !guidedEndSweepId.value) {
    guidedEndSweepId.value = list[list.length - 1]?.id ?? ''
  }
}, { immediate: true })

async function startGuidedTour() {
  const startId = currentSweep.value?.id
  const endId = guidedEndSweepId.value
  if (!startId || !endId || startId === endId) return
  guidedPlaying.value = true
  try {
    const path = await findPath(startId, endId)
    if (path.length > 1) await playPath(path, 2500)
    else if (path.length === 1) await moveTo(path[0])
  } catch (_) {
    // ignore
  } finally {
    guidedPlaying.value = false
  }
}
</script>

<style scoped>
.tour-viewer {
  position: absolute;
  inset: 0;
}
.showcase-iframe {
  width: 100%;
  height: 100%;
  border: none;
}
.sdk-error {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.85);
  color: #fff;
  padding: 2rem;
  text-align: center;
  z-index: 5;
}
.sdk-error-detail {
  margin: 0.5rem 0 0;
  font-size: 0.9rem;
  opacity: 0.85;
}
.guided-tour {
  position: fixed;
  top: 1rem;
  left: 50%;
  transform: translateX(-50%);
  z-index: 10;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(255, 255, 255, 0.95);
  padding: 0.5rem 0.75rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
.guided-tour-label {
  font-size: 0.85rem;
  font-weight: 500;
}
.guided-tour-select {
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
  border-radius: 4px;
  min-width: 120px;
}
.guided-tour-btn {
  padding: 0.35rem 0.75rem;
  font-size: 0.85rem;
  cursor: pointer;
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 4px;
}
.guided-tour-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
