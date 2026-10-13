import type { H3Event } from 'h3'

export function loginWithRedirect(event: H3Event, redirectTo: string) {
  setCookie(event, 'auth-return-to', redirectTo)
  return sendRedirect(event, '/api/auth/keycloak')
}
