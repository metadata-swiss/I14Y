<template>
  <OdsPage>
    <template #header>
      <OdsBreadcrumbs :breadcrumbs="breadcrumbs" />
    </template>
    <main id="main-content">
      <!-- search panel -->
      <section
        id="search-results"
        class="section section--default"
      >
        <div class="container gap--responsive">
          <div
            class="search-results search-results--grid"
            aria-live="polite"
            aria-busy="false"
          >
            <div class="ods-card-list">
              <ul class="search-results-list">
                <li
                  v-for="category in getSearchResultsEnhanced"
                  :key="category.id"
                >
                  <OdsCard
                    style="height: 100%;"
                    :title="getCurrentTranslation(category.pref_label, locale)"
                    clickable
                  >
                    <template #footer-action>
                      <SvgIcon
                        icon="Trash"
                        role="btn"
                      />
                    </template>
                  </OdsCard>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>
    </main>
  </OdsPage>
</template>

<script setup lang="ts">
import OdsPage from '../../app/components/OdsPage.vue'
import OdsBreadcrumbs from '../../app/components/OdsBreadcrumbs.vue'
import { homePageBreadcrumb } from '../../app/composables/breadcrumbs'
import { useVocabularySearch } from '../../app/piveau/vocabularies'
import { getCurrentTranslation } from '../../app/lib/getCurrentTranslation'
import OdsCard from '../../app/components/content/OdsCard.vue'
import SvgIcon from '../../app/components/SvgIcon.vue'

definePageMeta({
  middleware: 'require-auth',
})

const { t, locale } = useI18n()

const { useSearch } = useVocabularySearch()
const { query, getSearchResultsEnhanced } = useSearch({
  queryParams: {
    limit: 100,
    vocabulary: 'data-theme',
  },
})

await query.suspense()

const breadcrumbs = [
  await homePageBreadcrumb(locale),
  {
    title: t('message.header.navigation.admin.title'),
  },
  {
    title: t('message.header.navigation.admin.categories'),
  },
]
</script>
