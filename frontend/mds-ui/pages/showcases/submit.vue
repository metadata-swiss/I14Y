<template>
  <OdsPage :page="{ title }">
    <template #header>
      <OdsNotificationBanner
        :open="error"
        type="error"
      >
        {{ t('failure_message') }}

        <pre>{{ submissionError }}</pre>
        <ul v-if="Array.isArray(submissionValidationIssues)">
          <li
            v-for="issue in submissionValidationIssues"
            :key="issue.path.join('-')"
          >
            {{ issue.path.join('.') }}: {{ issue.message }}
          </li>
        </ul>
        <p v-else>
          {{ submissionValidationIssues.error }}
        </p>

        <template #buttons>
          <OdsButton
            variant="outline"
            :title="i18n.t('message.header.navigation.close')"
            icon-right
            icon="Checkmark"
            @click="closeMessages"
          />
        </template>
      </OdsNotificationBanner>
    </template>

    <section class="section section--py">
      <div class="container">
        <form
          ref="newShowcaseForm"
          class="form"
          method="post"
          @submit="submit"
        >
          <section class="preline">
            {{ t('paragraph.top') }}
          </section>
          <h2 class="h2">
            {{ t('header.showcase_information') }}
          </h2>
          <div class="form__group">
            <OdsInput
              id="title"
              :label="t('field.title')"
              required
            />
            <OdsInput
              id="url"
              :label="t('field.url')"
              required
            />
          </div>
          <div class="form__group">
            <OdsSelect
              id="type"
              name="type"
              :label="t('field.type')"
              required
            >
              <option
                v-for="type in showcaseType"
                :key="type.id"
                :value="type.id"
              >
                {{ type.title }}
              </option>
            </OdsSelect>
            <div class="form__group">
              <OdsInput
                id="images"
                type="file"
                :label="t('field.images')"
                accept="image/*"
                required
                multiple
              />
            </div>
            <div class="form__group">
              <OdsMultiSelect
                id="dataset"
                :label="t('field.datasets.label')"
                :load-options="searchDatasets"
                :close-on-select="false"
                :options="datasets"
              >
                <template #no-options>
                  {{ t('field.datasets.prompt') }}
                </template>
                <template #selected-option="option">
                  {{ option.title }}
                  <input
                    type="hidden"
                    :name="`datasets[${option.id}]`"
                    :value="option.title"
                  >
                </template>
              </OdsMultiSelect>
            </div>
            <OdsMultiSelect
              :label="t('field.themes.label')"
              :options="dataThemes"
              :close-on-select="false"
              required
            >
              <template #no-options>
                {{ t('field.themes.prompt') }}
              </template>
              <template #selected-option="option">
                {{ option.title }}
                <input
                  type="hidden"
                  name="themes"
                  :value="option.id"
                >
              </template>
            </OdsMultiSelect>
            <OdsInput
              id="keywords"
              :label="t('field.keywords.label')"
              :placeholder="t('field.keywords.prompt')"
            />
            <OdsInput
              id="createdBy"
              :label="t('field.created_by')"
              required
            />
          </div>

          <h2 class="h2">
            {{ t('header.contact') }}
          </h2>
          <div class="form__group">
            <div class="form__group">
              <OdsInput
                id="contactDetails.name"
                :label="t('field.contact_details.name')"
                required
              />
            </div>
            <div class="form__group">
              <OdsInput
                id="contactDetails.email"
                :label="t('field.contact_details.email')"
                type="email"
                required
              />
            </div>
            <div class="form__group">
              <OdsInput
                id="contactDetails.github"
                :label="t('field.contact_details.github')"
              />
            </div>
          </div>

          <h2 class="h2">
            {{ t('header.more') }}
          </h2>
          <div>
            <p class="preline">
              {{ t('paragraph.more') }}
            </p>
          </div>

          <div class="form__group">
            <div class="form__group">
              <OdsTextarea
                id="more.whoAreYou"
                :label="t('field.more.who_are_you.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <li>
                      <I18nT
                        keypath="message.showcase.submission_form.field.more.who_are_you.organization"
                        tag="b"
                      />
                    </li>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.who_are_you.mission"
                      tag="li"
                    >
                      <template #mainObjective>
                        <b>{{ t('field.more.who_are_you.mainObjective') }}</b>
                      </template>
                    </I18nT>
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.goal"
                :label="t('field.more.goal.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <li>
                      <I18nT
                        keypath="message.showcase.submission_form.field.more.goal.hint"
                        tag="b"
                      />
                    </li>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.goal.help"
                      tag="li"
                    />
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.why"
                :label="t('field.more.why.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.why.hint"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.why.what"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.why.challenges"
                      tag="li"
                    />
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.users"
                :label="t('field.more.users.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.users.how"
                      tag="li"
                    >
                      <template #usefulness>
                        <b>{{ t('field.more.users.usefulness') }}</b>
                      </template>
                    </I18nT>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.users.benefits"
                      tag="li"
                    >
                      <template #whichFields>
                        <b>{{ t('field.more.users.whichFields') }}</b>
                      </template>
                    </I18nT>
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.challengesBefore"
                :label="t('field.more.challengesBefore.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesBefore.hint"
                      tag="li"
                    >
                      <template #majorChallenges>
                        <b>{{ t('field.more.challengesBefore.majorChallenges') }}</b>
                      </template>
                    </I18nT>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesBefore.overcome"
                      tag="li"
                    >
                      <template #overcomeHow>
                        <b>{{ t('field.more.challengesBefore.overcomeHow') }}</b>
                      </template>
                    </I18nT>
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.challengesCreating"
                :label="t('field.more.challengesCreating.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesCreating.preparing"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesCreating.finding"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesCreating.identifying"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesCreating.accessing"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.challengesCreating.preparing"
                      tag="li"
                    />
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.getMost"
                :label="t('field.more.getMost.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.getMost.access"
                      tag="li"
                    />
                    <I18nT
                      keypath="message.showcase.submission_form.field.more.getMost.conditions"
                      tag="li"
                    />
                    <li>
                      <I18nT
                        keypath="message.showcase.submission_form.field.more.getMost.tips"
                        tag="b"
                      />
                    </li>
                  </ul>
                </template>
              </OdsTextarea>
            </div>
            <div class="form__group">
              <OdsTextarea
                id="more.anythingElse"
                :label="t('field.more.anythingElse.label')"
                message-type="info"
              >
                <template #message>
                  <ul>
                    <li>
                      <I18nT
                        keypath="message.showcase.submission_form.field.more.anythingElse.mentionHint"
                      >
                        <template #hashtags>
                          <b>{{ t('field.more.anythingElse.hashtags') }}</b>
                        </template>
                        <template #people>
                          <b>{{ t('field.more.anythingElse.people') }}</b>
                        </template>
                      </I18nT>
                    </li>
                    <li>
                      <I18nT
                        keypath="message.showcase.submission_form.field.more.anythingElse.visualsHint"
                        tag="b"
                      >
                        <template #visuals>
                          <b>{{ t('field.more.anythingElse.visuals') }}</b>
                        </template>
                      </I18nT>
                    </li>
                  </ul>
                </template>
              </OdsTextarea>
            </div>
          </div>

          <div class="form__group">
            <OdsButton
              submit
              variant="outline-negative"
              :title="t('submit_button')"
              icon-right
              :style="submitting ? 'pointer-events: none; cursor: wait' : ''"
            >
              <template #icon>
                <SvgIcon
                  v-if="submitting"
                  icon="Spinner"
                  class="btn__icon btn__icon--spin"
                />
                <SvgIcon
                  v-else
                  icon="Checkmark"
                  class="btn__icon"
                />
              </template>
            </OdsButton>
          </div>
        </form>
      </div>
    </section>
  </OdsPage>
</template>

<script setup lang="ts">
import { reactive, ref, toRefs } from 'vue'
import type { $ZodIssue as ZodIssue } from 'zod/v4/core'
import type { SearchParamsBase } from '@piveau/sdk-core/hubSearch'
import { debounce } from 'perfect-debounce'
import OdsMultiSelect from '../../app/components/dataset/OdsMultiSelect.vue'
import { getCurrentTranslation } from '../../app/lib/getCurrentTranslation'
import { useDatasetsSearch } from '../../app/piveau/datasets'
import { useVocabularySearch } from '../../app/piveau/vocabularies'
import OdsNotificationBanner from '../../app/components/OdsNotificationBanner.vue'
import OdsButton from '../../app/components/OdsButton.vue'
import OdsInput from '../../app/components/OdsInput.vue'
import OdsSelect from '../../app/components/OdsSelect.vue'
import OdsPage from '../../app/components/OdsPage.vue'
import SvgIcon from '../../app/components/SvgIcon.vue'
import OdsTextarea from '../../app/components/OdsTextarea.vue'
import { I18nT } from 'vue-i18n'

const i18n = useI18n()
const { locale } = i18n
const t = (key: string) => i18n.t(`message.showcase.submission_form.${key}`)

const { useSearch } = useVocabularySearch()
const searchDataThemes = useSearch({
  queryParams: {
    limit: 100,
    vocabulary: 'data-theme',
  },
})

const dataThemes = computed(() => {
  return searchDataThemes.getSearchResultsEnhanced.value.map(item => ({
    id: item.resource,
    title: item.pref_label ? getCurrentTranslation(item.pref_label, locale.value) : '',
  }))
})

const searchShowcaseTypes = useSearch({
  queryParams: {
    vocabulary: 'showcase-types',
  },
})

const showcaseType = computed(() => {
  return searchShowcaseTypes.getSearchResultsEnhanced.value.map(item => ({
    id: item.resource,
    title: item.pref_label ? getCurrentTranslation(item.pref_label, locale.value) : '',
  }))
})

const title = t('title')
useSeoMeta({ title: `${title} | opendata.swiss` })

const submitting = ref(false)
const error = ref<boolean>(false)
const submissionError = ref<string | null>(null)
const submissionValidationIssues = ref<{ error: string } | ZodIssue[]>([])

const newShowcaseForm = ref<HTMLFormElement | null>(null)

async function submit(e: Event) {
  e.preventDefault()

  if (submitting.value) {
    return
  }

  try {
    submitting.value = true
    const response = await fetch('/api/showcases', {
      method: 'POST',
      body: new FormData(newShowcaseForm.value!),
      headers: {
        'Accept-Language': locale.value,
      },
    })
    if (response.ok) {
      error.value = false
      newShowcaseForm.value?.reset()
    }
    else if (response.status === 400 || response.status === 409) {
      submissionError.value = 'Form contains invalid data:'
      submissionValidationIssues.value = await response.json()
      error.value = true
    }
    else {
      submissionError.value = `Server responded with: ${response.status} - ${response.statusText}`
      error.value = true
    }
  }
  catch (e) {
    submissionError.value = `${e.message}\n${e.stack}`
    error.value = true
  }
  finally {
    window.scrollTo({ top: 0, behavior: 'smooth' })
    submitting.value = false
  }
}

function closeMessages() {
  error.value = false
  submissionError.value = null
}

const searchTerm = ref()
const datasetQueryParams: SearchParamsBase = reactive({
  limit: 10,
  q: searchTerm,
  sort: 'relevance',
})
const { useSearch: datasetSearch } = useDatasetsSearch()
const { query, getSearchResultsEnhanced } = datasetSearch({
  queryParams: toRefs(datasetQueryParams),
})

const datasets = ref([] as { id: string, title: string }[])
const searchDatasets = debounce(async function (arg: string, loading: (arg: boolean) => void) {
  if (arg.length === 0) {
    return
  }

  loading(true)
  searchTerm.value = arg
  await query.suspense()
  datasets.value = getSearchResultsEnhanced.value.map(dataset => ({
    id: dataset.getResource,
    title: dataset.getTitle,
  }))
  loading(false)
}, 300)
</script>

<style scoped>
.btn__icon--spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.preline {
  white-space: pre-line;
}
</style>
