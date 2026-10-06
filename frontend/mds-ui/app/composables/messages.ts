export function useMessages() {
  const messageCookie = useCookie('message')
  const message = computed(() => messageCookie.value)

  const errorMessageCookie = useCookie('message.error')
  const errorMessage = computed(() => errorMessageCookie.value)

  function closeMessages() {
    messageCookie.value = undefined
  }

  function closeErrorMessages() {
    errorMessageCookie.value = undefined
  }

  return {
    message,
    errorMessage,
    closeMessages,
    closeErrorMessages,
  }
}

// Ensures the router hook is only registered once
let autoClearRegistered = false

export function useAutoClearMessagesOnRouteChange() {
  if (autoClearRegistered) return
  autoClearRegistered = true

  const router = useRouter()

  // Wait until app is ready on client before touching router hooks
  onNuxtReady(() => {
    router.afterEach(() => {
      const { closeMessages, closeErrorMessages } = useMessages()
      closeMessages()
      closeErrorMessages()
    })
  })
}
