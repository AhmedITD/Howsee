<template>
  <div class="tour-viewer">
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
  ready,
  error: sdkError
} = useMatterportSdk(iframeRef, props.applicationKey)

async function onIframeLoad() {
  await loadSdk()
}

watch(ready, (isReady) => {
  if (isReady && props.startSweepId) {
    moveTo(props.startSweepId).catch(() => {})
  }
})
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
</style>
