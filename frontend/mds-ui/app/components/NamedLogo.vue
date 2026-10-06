<template>
  <NuxtImg
    src="/img/logo_horizontal.svg"
    :alt="t('message.header.ods_logo')"
    :title="t('message.header.ods_logo')"
    fetchpriority="high"
    class="logo__freebrand"
  />
</template>

<script lang="ts" setup>
import { useI18n } from '#imports'

const { t } = useI18n()
</script>

<style scoped>
.logo__freebrand {
  display: none;
}

@media (min-width: 1280px) {
    .logo__freebrand {
        display: block;
    }
}
</style>
