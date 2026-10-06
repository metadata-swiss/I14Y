import type { User } from '#auth-utils'

declare module '#auth-utils' {
  interface User {
    name: string
    email: string
    sub: string
    preferred_username: string
  }
}

export default defineOAuthKeycloakEventHandler({
  async onSuccess(event, { user }: { user: User }) {
    await setUserSession(event, {
      user: {
        name: user.name || user.preferred_username || user.email || user.sub,
        email: user.email,
      } as User,
    })

    const returnTo = getCookie(event, 'auth-return-to') || '/'
    deleteCookie(event, 'auth-return-to')

    return sendRedirect(event, returnTo)
  },
  onError(event, { message }) {
    return sendRedirect(event, '/login?error=' + encodeURIComponent(message || 'OAuth Error'))
  },
})
