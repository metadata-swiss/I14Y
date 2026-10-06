/** * Vue component for displaying an svg. Workaround in order to have svg inline rather than as a
href * due to file path complications when used as a plugin by other apps. * When built, this svg
file will be provided inline. */

<template>
  <NuxtImg
    src="/img/logo.svg"
    :alt="t('message.header.ods_logo')"
    :title="t('message.header.ods_logo')"
    fetchpriority="high"
    class="logo__freebrand" />
</template>

<script lang="ts" setup>
import { useI18n } from '#imports'

const { t } = useI18n()
</script>

<style lang="scss" scoped>
@use '@/assets/ods/ods_breakpoints.scss' as mdeia;

.logo__freebrand {
  display: block;
}

@media (min-width: 1280px) {
    .logo__freebrand {
        display: none;
    }
}

@media (min-width: 768px) {
    .logo__freebrand {
        height: 60px;
    }
}
</style>
