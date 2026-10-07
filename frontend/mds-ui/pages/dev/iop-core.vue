<script setup lang="ts">
// Temporary check for the generated iop-core client (V1 phase 1). Remove when the first real page uses it.
import { ref } from 'vue'
import { useSeoMeta } from 'nuxt/app'
import type { ErrorType } from '~~/api-client/iop-core-fetch'
import { getAgentsById, getAgentsStatistics, useGetAgents, type ProblemDetails } from '~~/api-client/generated/iop-core'

useSeoMeta({ title: 'iop-core client check | opendata.swiss' })

// Vue Query composable, rendered on the server like the existing pages (await suspense()).
// Changing `page` loads the next page in the browser.
const page = ref(1)
const agentsQuery = useGetAgents(() => ({ page: page.value, pageSize: 5 }))
await agentsQuery.suspense()
const { data: agents, error, isFetching } = agentsQuery

// Plain functions, called from the browser on click.
const statistics = ref<string>()
async function loadStatistics() {
  try {
    const result = await getAgentsStatistics()
    statistics.value = `${result.length} publishers with statistics`
  }
  catch (e) {
    statistics.value = `Error: ${e instanceof Error ? e.message : e}`
  }
}

const missingAgent = ref<string>()
async function loadMissingAgent() {
  try {
    await getAgentsById('00000000-0000-0000-0000-000000000000')
    missingAgent.value = 'Unexpected success'
  }
  catch (e) {
    const { statusCode, data } = e as ErrorType<ProblemDetails>
    missingAgent.value = `${statusCode} ${data?.title}`
  }
}
</script>

<template>
  <main class="container">
    <h1>iop-core client check</h1>

    <h2>GET /api/Agents, page {{ page }} (server-side rendering, Vue Query)</h2>
    <p v-if="error">
      Error: {{ error.message }}
    </p>
    <ul
      v-else
      data-testid="agents"
    >
      <li
        v-for="agent in agents"
        :key="agent.id"
      >
        {{ agent.name.de ?? agent.name.en ?? agent.identifier }} ({{ agent.identifier }})
      </li>
    </ul>
    <button
      type="button"
      data-testid="next-page"
      :disabled="isFetching"
      @click="page++"
    >
      Next page
    </button>

    <h2>GET /api/Agents/statistics (browser)</h2>
    <button
      type="button"
      data-testid="load-statistics"
      @click="loadStatistics"
    >
      Load in browser
    </button>
    <p data-testid="statistics">
      {{ statistics }}
    </p>

    <h2>GET /api/Agents/{id} with an unknown id (browser)</h2>
    <button
      type="button"
      data-testid="load-missing-agent"
      @click="loadMissingAgent"
    >
      Load in browser
    </button>
    <p data-testid="missing-agent">
      {{ missingAgent }}
    </p>
  </main>
</template>
