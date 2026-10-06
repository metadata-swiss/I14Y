<template>
  <footer class="bg--secondary-600">
    <div class="container">
      <div class="footer-information">
        <div class="footer-information__entry">
          <h3>{{ t('message.ods-footer.block_1.title') }}</h3>
          <p>{{ t('message.ods-footer.block_1.paragraph_1') }}</p>
          <p>© opendata.swiss</p>
        </div>
        <div class="footer-information__entry">
          <h3>{{ t('message.ods-footer.block_2.title') }}</h3>
          <div class="footer-information__social">
            <a
              class="footer__link"
              href="https://www.facebook.com/people/opendataswiss/61558495974785/"
              target="_blank"
            >
              <SvgIcon
                icon="Facebook"
                class="icon icon--base footer-information__icon"
              />
              Facebook
            </a>
            <a
              class="footer__link"
              href="https://www.linkedin.com/showcase/opendata-swiss/"
              target="_blank"
            >
              <SvgIcon
                icon="LinkedIn"
                class="icon icon--base footer-information__icon"
              />
              LinkedIn</a>
            <a
              class="footer__link"
              href="https://www.instagram.com/opendata.swiss/"
              target="_blank"
            >
              <SvgIcon
                icon="Instagram"
                class="icon icon--base footer-information__icon"
              />Instagram</a>
            <a
              class="footer__link"
              href="https://x.com/opendataswiss"
              target="_blank"
            >
              <SvgIcon
                icon="Twitter"
                class="icon icon--base footer-information__icon"
              />𝕏</a>
          </div>
          <OdsButton
            :title="t('message.ods-footer.block_2.news_mail_aria_label')"
            :aria-label="t('message.ods-footer.block_2.news_mail_aria_label')"
            icon="ArrowRight"
            icon-right
            size="sm"
            variant="outline-negative"
            @click="openLinkInNewTab(t('message.ods-footer.block_2.news_mail_href')) "
          >
            <span>NewsMail</span>
          </OdsButton>
        </div>

        <div class="footer-information__entry">
          <h3>{{ t('message.ods-footer.block_3.title') }}</h3>
          <div>
            foobar bar bar bar bar bar bar bar bar bar bar bar bar
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { useI18n } from '#imports'
import OdsButton from '../OdsButton.vue'
import SvgIcon from '../SvgIcon.vue'

const { t } = useI18n()

function openLinkInNewTab(url: string) {
  window.open(url, '_blank')
}
</script>

<style lang="scss" scoped>
.footer-information__icon {
  font-size: 18px;
}

a {
  text-decoration: none !important;
}
</style>
