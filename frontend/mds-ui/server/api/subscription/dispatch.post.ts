import listmonk from '#server/lib/listmonk/index.js'
import { HubSearch } from '#server/lib/piveau'
import { dispatchDigest } from '#server/lib/subscription/dispatch'

export default defineEventHandler(async (event) => {
  const {
    public: {
      piveauHubSearchUrl: baseUrl,
    },
    listmonk: listmonkConfig,
    appUrl,
    subscription: {
      datasetQueryBatchSize: queryPageLimit,
      maxDatasetsPerEmail,
    },
  } = useRuntimeConfig()
  const body = await readFormData(event)
  const digest = body.get('digest')?.toString()

  if (!digest || (digest !== 'daily' && digest !== 'weekly')) {
    throw createError({ statusCode: 400, statusMessage: 'Missing or invalid digest parameter. Must be "daily" or "weekly"' })
  }

  const piveau = new HubSearch(baseUrl)
  const Listmonk = new listmonk(listmonkConfig)
  try {
    const { emailsSent, emailsFailed } = await dispatchDigest(digest, {
      piveau,
      listmonk: Listmonk,
      appUrl,
      key: listmonkConfig.preferences.hmac_key,
      queryPageLimit,
      maxDatasetsPerEmail,
    })

    return {
      message: `Subscriber emails: sent=${emailsSent} failed=${emailsFailed}`,
    }
  }
  catch (err: unknown) {
    return createError({
      statusCode: 500,
      message: 'Failed to fetch latest datasets',
      cause: err instanceof Error ? err?.cause : undefined,
    })
  }
})
