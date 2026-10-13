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
                  v-for="category in categories"
                  :key="category.code ?? ''"
                >
                  <OdsCard
                    style="height: 100%;"
                    :title="localize(category.name, locale)"
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
import OdsPage from '~/components/OdsPage.vue'
import OdsBreadcrumbs from '~/components/OdsBreadcrumbs.vue'
import { homePageBreadcrumb } from '~/composables/breadcrumbs'
import { useCategories } from '~/composables/useDatasets'
import { localize } from '~/utils/getCurrentTranslation'
import OdsCard from '~/components/content/OdsCard.vue'
import SvgIcon from '~/components/SvgIcon.vue'

definePageMeta({
  middleware: 'require-auth',
})

const { t, locale } = useI18n()

const { query, categories } = useCategories()

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
