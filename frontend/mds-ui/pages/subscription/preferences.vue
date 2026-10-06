<template>
  <OdsPage>
    <main id="main-content">
      <!-- search panel -->
      <section
        id="search-results"
        class="section section--default"
      >
        <div class="container gap--responsive">
          <h2 class="h2">
            {{ t('message.subscribe.preferences.title') }}
          </h2>
          <section>
            <form ref="form">
              <OdsRadioGroup :legend="t('message.subscribe.preferences.legend')">
                <OdsRadio
                  id="daily"
                  name="frequency"
                  value="daily"
                  :label="t('message.subscribe.preferences.frequency.daily')"
                  :checked="frequency === 'daily'"
                />
                <OdsRadio
                  id="weekly"
                  name="frequency"
                  value="weekly"
                  :label="t('message.subscribe.preferences.frequency.weekly')"
                  :checked="frequency === 'weekly'"
                />
              </OdsRadioGroup>

              <OdsSelect
                id="language"
                v-model="language"
                name="language"
                fit-content
                :label="t('message.subscribe.preferences.language_label')"
              >
                <option
                  v-for="lang in APP_LANGUAGES"
                  :key="lang"
                  :value="lang"
                >
                  {{ t('message.languages.' + lang) }}
                </option>
              </OdsSelect>

              <OdsFormField
                v-if="datasets.length > 0"
                :label="t('message.subscribe.preferences.datasets_label')"
                for="dataset"
              >
                <OdsCheckbox
                  v-for="dataset in datasets"
                  :key="dataset.id"
                  name="dataset"
                  :value="dataset.id"
                  :label="dataset.name"
                  checked
                />
              </OdsFormField>

              <OdsFormField
                v-if="categories.length > 0"
                :label="t('message.subscribe.preferences.categories_label')"
                for="dataset"
              >
                <OdsCheckbox
                  v-for="category in categories"
                  :key="category.id"
                  name="category"
                  :value="category.id"
                  :label="getCurrentTranslation(category.pref_label, locale)"
                  checked
                />
              </OdsFormField>
              <OdsButton
                :title="t('message.subscribe.preferences.update_button')"
                variant="outline"
                @click="updatePreferences"
              />
            </form>
          </section>
          <section>
            <h2 class="h2">
              {{ t('message.subscribe.preferences.unsubscribe_title') }}
            </h2>
            <OdsButton
              :title="t('message.subscribe.preferences.unsubscribe_button')"
              variant="outline"
            />
          </section>
        </div>
      </section>
    </main>
  </OdsPage>
</template>

<script setup lang="ts">
import OdsPage from '../../app/components/OdsPage.vue'
import OdsRadioGroup from '../../app/components/OdsRadioGroup.vue'
import OdsRadio from '../../app/components/OdsRadio.vue'
import OdsCheckbox from '../../app/components/OdsCheckbox.vue'
import OdsFormField from '../../app/components/OdsFormField.vue'
import OdsSelect from '../../app/components/OdsSelect.vue'
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { useDatasetsSearch } from '../../app/piveau/datasets.js'
import OdsButton from '../../app/components/OdsButton.vue'
import { getCurrentTranslation } from '../../app/lib/getCurrentTranslation'
import { useVocabularySearch } from '../../app/piveau/vocabularies'
import { APP_LANGUAGES, type AppLanguage } from '../../app/constants/langages'

const route = useRoute()
const router = useRouter()

const form = ref<HTMLFormElement>()
const datasets = ref([])
const categories = ref([])
const frequency = ref()
const language = ref<AppLanguage>()
const { t, locale } = useI18n()

const query = {
  id: route.query.id,
  token: route.query.token,
}
const { data } = await useFetch('/api/subscription/preferences', { query })
if (!data.value) {
  router.replace('/404')
}

const preferences = data.value.preferences

const { useResource } = useDatasetsSearch()
const { useSearch } = useVocabularySearch()
const vocabSearch = useSearch({
  queryParams: {
    limit: 100,
    vocabulary: 'data-theme',
  },
})

frequency.value = preferences.frequency || 'daily'
language.value = (preferences.language as AppLanguage) || APP_LANGUAGES[0]

const loadCategories = (preferences.categories || []).map(async (category) => {
  await vocabSearch.query.suspense()
  return vocabSearch.getSearchResultsEnhanced.value
    .filter(result => result.id === category)
    .shift()
})
categories.value = (await Promise.all(loadCategories)).filter(Boolean)

const datasetsLoaded = (preferences.datasets || []).map(async (id) => {
  const { query, resultEnhanced } = useResource(id)
  await query.suspense()
  return {
    id,
    name: resultEnhanced.value.getTitle,
  }
})

datasets.value = (await Promise.all(datasetsLoaded)).sort((a, b) => a.name.localeCompare(b.name))

const message = useCookie('message')
const errorMessage = useCookie('message-error')

async function updatePreferences() {
  const { data, error } = await useFetch('/api/subscription/preferences', {
    query,
    method: 'PUT',
    body: new URLSearchParams(new FormData(form.value!) as unknown as Array<[string, string]>),
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  })

  if (error) {
    message.value = 'subscribe.preferences.updated'
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  }
  else {
    console.error(data)
    errorMessage.value = 'subscribe.preferences.update_failed'
  }
}

useSeoMeta({
  title: `${t('message.subscribe.preferences.page_title')} | opendata.swiss`,
})
</script>

<style scoped>
  .form-group, section+section {
    padding-top: 30px
  }
</style>
