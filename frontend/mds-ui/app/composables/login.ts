export function useLoginWithRedirect() {
  const route = useRoute()
  const authReturnTo = useCookie('auth-return-to')

  return (redirectTo?: string) => {
    authReturnTo.value = redirectTo || route.fullPath
    window.location.href = '/api/auth/keycloak'
  }
}
