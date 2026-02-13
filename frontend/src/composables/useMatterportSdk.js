import { ref, onUnmounted } from 'vue'

const SDK_BOOTSTRAP_VERSION = '3.0.0-0-g0517b8d76c'

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
  let unsubscribeCurrent = null
  let unsubscribeData = null

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
        unsubscribeCurrent = sdk.Sweep.current.subscribe((sweep) => {
          currentSweep.value = { sid: sweep?.sid ?? '', id: sweep?.id ?? '' }
        })
      }
      if (sdk.Sweep?.data?.subscribe) {
        const list = []
        unsubscribeData = sdk.Sweep.data.subscribe({
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

  onUnmounted(() => {
    if (typeof unsubscribeCurrent === 'function') unsubscribeCurrent()
    if (typeof unsubscribeData === 'function') unsubscribeData()
  })

  return {
    loadSdk,
    mpSdk,
    currentSweep,
    sweeps,
    moveTo,
    ready,
    error
  }
}
