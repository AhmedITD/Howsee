import { ref, onUnmounted } from 'vue'

// Centralized SDK version for easy upgrades; see https://matterport.github.io/showcase-sdk/sdk_changelog.html
const SDK_BOOTSTRAP_VERSION = import.meta.env.VITE_MATTERPORT_SDK_VERSION || '3.0.0-0-g0517b8d76c'

/**
 * Load Matterport SDK and connect to the Showcase iframe.
 * @param {Ref<HTMLIFrameElement|null>} iframeRef - Template ref of the iframe
 * @param {string} applicationKey - Matterport application key
 * @returns {Promise<{ mpSdk: any, currentSweep: Ref<Object>, sweeps: Ref<Array>, moveTo: (sweepId: string, options?: Object) => Promise<string>, ready: Ref<boolean>, error: Ref<string|null> }>}
 */
export function useMatterportSdk(iframeRef, applicationKey) {
  const ready = ref(false)
  const error = ref(null)
  const mpSdk = ref(null)
  const currentSweep = ref({ sid: '', id: '' })
  const sweeps = ref([])
  let subscriptionCurrent = null
  let subscriptionData = null

  function cancelSubscription(sub) {
    if (sub && typeof sub.cancel === 'function') sub.cancel()
    else if (typeof sub === 'function') sub()
  }

  async function loadSdk() {
    if (!applicationKey) {
      error.value = 'Missing Matterport application key'
      return
    }
    const sdkUrl = `https://api.matterport.com/sdk/bootstrap/${SDK_BOOTSTRAP_VERSION}/sdk.es6.js?applicationKey=${encodeURIComponent(applicationKey)}`
    try {
      const mod = await import(/* @vite-ignore */ sdkUrl)
      const connect = mod.connect
      if (typeof connect !== 'function') {
        error.value = 'SDK connect not available'
        return
      }
      const iframe = iframeRef?.value ?? iframeRef
      if (!iframe) {
        error.value = 'Iframe ref not set'
        return
      }
      const sdk = await connect(iframe)
      mpSdk.value = sdk

      if (sdk.Sweep?.current?.subscribe) {
        subscriptionCurrent = sdk.Sweep.current.subscribe((sweep) => {
          currentSweep.value = { sid: sweep?.sid ?? '', id: sweep?.id ?? '' }
        })
      }
      if (sdk.Sweep?.data?.subscribe) {
        const list = []
        subscriptionData = sdk.Sweep.data.subscribe({
          onAdded(index, item) {
            if (item) list.push({ id: item.id, sid: item.sid })
            sweeps.value = [...list]
          },
          onRemoved(index) {
            list.splice(index, 1)
            sweeps.value = [...list]
          },
          onCollectionUpdated(collection) {
            if (collection && typeof collection.values === 'function') {
              sweeps.value = Array.from(collection.values()).map((item) => ({
                id: item?.id,
                sid: item?.sid
              }))
            }
          }
        })
      }
      ready.value = true
    } catch (e) {
      error.value = e?.message ?? 'Failed to load Matterport SDK'
    }
  }

  async function moveTo(sweepId, options = {}) {
    const sdk = mpSdk.value
    if (!sdk?.Sweep?.moveTo) return Promise.reject(new Error('SDK not ready'))
    const transition = sdk.Camera?.Transition?.FLY ?? 'fly'
    return sdk.Sweep.moveTo(sweepId, {
      transition,
      transitionTime: 1500,
      ...options
    })
  }

  /**
   * Create a sweep graph for pathfinding. Call dispose() when done.
   * @returns {Promise<{ graph: any, dispose: () => void } | null>}
   */
  async function createSweepGraph() {
    const sdk = mpSdk.value
    if (!sdk?.Sweep?.createGraph) return null
    try {
      const graph = await sdk.Sweep.createGraph()
      return {
        graph,
        dispose() {
          if (graph && typeof graph.dispose === 'function') graph.dispose()
        }
      }
    } catch (_) {
      return null
    }
  }

  /**
   * Find path between two sweeps using A*. Returns ordered list of sweep IDs.
   * @param {string} startSweepId
   * @param {string} endSweepId
   * @returns {Promise<string[]>}
   */
  async function findPath(startSweepId, endSweepId) {
    const sweepGraph = await createSweepGraph()
    if (!sweepGraph) return []
    try {
      const sdk = mpSdk.value
      if (!sdk?.Graph?.createAStarRunner) return []
      const startVertex = sweepGraph.graph.vertex(startSweepId)
      const endVertex = sweepGraph.graph.vertex(endSweepId)
      if (!startVertex || !endVertex) return []
      const runner = sdk.Graph.createAStarRunner(sweepGraph.graph, startVertex, endVertex)
      const result = runner.exec()
      const path = result?.path
      if (!path || !Array.isArray(path)) return []
      return path.map((v) => (v && (v.id ?? v.data?.id)) || '').filter(Boolean)
    } finally {
      sweepGraph.dispose()
    }
  }

  /**
   * Play a guided tour by moving through sweep IDs with a delay between each.
   * @param {string[]} sweepIds
   * @param {number} delayMs
   * @returns {Promise<void>}
   */
  async function playPath(sweepIds, delayMs = 2500) {
    if (!Array.isArray(sweepIds) || sweepIds.length === 0) return
    for (const id of sweepIds) {
      await moveTo(id)
      if (delayMs > 0) await new Promise((r) => setTimeout(r, delayMs))
    }
  }

  onUnmounted(() => {
    cancelSubscription(subscriptionCurrent)
    cancelSubscription(subscriptionData)
  })

  return {
    loadSdk,
    mpSdk,
    currentSweep,
    sweeps,
    moveTo,
    createSweepGraph,
    findPath,
    playPath,
    ready,
    error
  }
}
