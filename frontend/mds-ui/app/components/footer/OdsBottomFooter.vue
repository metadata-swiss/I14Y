<template>
  <footer class="bg--secondary-700">
    <nav class="container" aria-label="Footer" tabindex="0" aria-hidden="false">
      <ul class="footer-navigation">
        <li>
          <NuxtLinkLocale to="/legal-framework" class="footer__link" :aria-label="t('message.bottom_footer.ods_aria_label_1')">
            {{ t('message.bottom_footer.ods_a_1') }}
          </NuxtLinkLocale>
        </li>
        <li><a class="footer__link" :href="t('message.bottom_footer.ods_href_2')" target="_blank" :aria-label="t('message.bottom_footer.ods_aria_label_2')">{{ t('message.bottom_footer.ods_a_2') }}</a></li>
      </ul>
    </nav>
  </footer>
</template>

<script lang="ts" setup>
import { useI18n } from '#imports'

const { t } = useI18n()
</script>

<style lang="scss" scoped>
a {
  text-decoration: none !important;
}
</style>
