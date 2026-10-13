export default defineNuxtRouteMiddleware((from, to) => {
  const { loggedIn } = useUserSession()
  const loginAndRedirect = useLoginWithRedirect()

  if (!loggedIn.value) {
    loginAndRedirect(to.fullPath)
  }
})
