import { createAppAuth } from '@octokit/auth-app'
import { defineNitroPlugin } from '#imports'

const executed = new WeakSet()

export default defineNitroPlugin(async (nitro) => {
  if (import.meta.dev) {
    console.warn('Skipping Showcase Harvesting trigger in development mode')
    return
  }

  if (process.env.DISABLE_SHOWCASE_HARVESTING === 'true') {
    console.warn('Showcase Harvesting trigger disabled with DISABLE_SHOWCASE_HARVESTING flag')
    return
  }

  if (executed.has(nitro)) {
    return
  }

  const environment = process.env.ENV
  if (!environment) {
    console.error('Cannot trigger showcase harvesting. ENV variable is missing')
    return
  }

  executed.add(nitro)

  const appId = process.env.GITHUB_APP_ID
  const installationId = process.env.GITHUB_APP_INSTALLATION_ID
  const privateKey = process.env.GITHUB_APP_PRIVATE_KEY
  const githubOrg = process.env.GITHUB_OWNER
  const appRepo = process.env.GITHUB_APP_REPO

  if (!appId || !installationId || !privateKey) {
    console.error('Missing required environment variables for GitHub App authentication')
    return
  }

  try {
    const auth = createAppAuth({
      appId,
      privateKey,
      installationId,
    })

    const { token } = await auth({ type: 'installation' })

    const response = await fetch(`https://api.github.com/repos/${githubOrg}/${appRepo}/actions/workflows/script-manual-trigger.yaml/dispatches`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/vnd.github+json',
        'Content-Type': 'application/json',
        'User-Agent': 'Nuxt-Nitro-Plugin',
      },
      body: JSON.stringify({
        ref: 'main',
        inputs: {
          environment,
          action: 'HARVEST_SHOWCASES',
        },
      }),
    })

    if (!response.ok) {
      const errorText = await response.text()
      console.error(`Failed to trigger GitHub Action: ${response.status} ${response.statusText}`, errorText)
    }
    else {
      console.info(`Successfully triggered GitHub Action for environment: ${environment}`)
    }
  }
  catch (e) {
    console.error('Error triggering GitHub Action:', e)
  }
})
