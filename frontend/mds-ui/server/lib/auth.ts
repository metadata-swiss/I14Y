interface Credentials {
  clientId: string
  clientSecret: string
}

export async function requestServiceAccountToken(serverUrl: string, realm: string, credentials: Credentials) {
  const tokenUrl = new URL(`/realms/${realm}/protocol/openid-connect/token`, serverUrl)
  const body = new URLSearchParams()
  body.append('grant_type', 'client_credentials')
  body.append('client_id', credentials.clientId)
  body.append('client_secret', credentials.clientSecret)

  const res = await fetch(tokenUrl, {
    method: 'POST',
    body,
    headers: {
      contentType: 'application/x-www-form-urlencoded',
    },
  })

  if (res.ok) {
    const data = await res.json()
    return data.access_token as string
  }

  throw new Error(`Failed to request service account token: ${res.status} ${res.statusText}`)
}
