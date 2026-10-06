<template>
  <section id="search-results" class="section section--default">
    <div class="container gap--responsive">
      <div class="search-results search-results--grid" aria-live="polite" aria-busy="false">
        <div class="search-results__header">
          <div class="search-results__header__left">
            <strong>{{ resultsCount }}</strong>
            {{ t('message.dataset_search.search_results') }}
          </div>

          <div class="search-results__header__right">
            <slot name="header-right" />
          </div>
        </div>
        <h2 class="sr-only">
          Results list
        </h2>
        <slot/>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
const { t } = useI18n()

interface PropTypes {
  resultsCount: number
}

defineProps<PropTypes>()
</script>
