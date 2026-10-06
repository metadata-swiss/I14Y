<template>
  <div class="distribution-item">
    <div class="content">
      <h3
        class="download-item__title"
        :title="props.distribution.title"
      >
        {{ props.distribution.title }}
      </h3>
      <p class="download-item__description" />

      <div class="footer">
        <p class="meta-info download-item__meta-info">
          <span
            v-if="props.distribution.format"
            class="meta-info__item"
          >{{ props.distribution.format }}
          </span>
          <span
            v-if="props.distribution.releaseDate"
            class="meta-info__item"
          >
            <OdsRelativeDateToggle :date="props.distribution.modificationDate ? props.distribution.modificationDate : props.distribution.releaseDate" />
          </span>
          <span
            v-if="props.distribution.formattedByteSize"
            class="meta-info__item"
          >{{ props.distribution.formattedByteSize }}
          </span>
        </p>
      </div>
    </div>
    <div class="call-to-action">
      <div>
        <a
          v-if="hasDownloadUrl"
          :href="distribution.downloadUrls[0]"
          target="_blank"
          rel="noopener"
          :title="t('message.dataset_detail.download')+ ' ' + props.distribution.format"
          class="download-item__link"
        >
          <SvgIcon
            icon="Download"
            size="sm"
            class="download-item__icon"
          />
        </a>
        <a
          v-if="hasAccessUrl && !hasDownloadUrl"
          :href="distribution.accessUrls[0]"
          target="_blank"
          rel="noopener"
          :title="t('message.dataset_detail.go_to_resource')+ ' ' + props.distribution.format"
          class="download-item__link"
        >
          <SvgIcon
            icon="External"
            size="sm"
            class="download-item__icon"
          />
        </a>
      </div>
      <div>
        <NuxtLinkLocale
          :to="`/datasets/${props.distribution.dataset.id}/distribution/${props.distribution.id}`"
          class="no-underline no-overvlow"
        >
          <OdsButton
            icon="ArrowRight"
            variant="outline"
            size="sm"
            class="download-item__button"
            icon-right
          >
            <span>{{ t('message.dataset_detail.details') }}</span>
          </OdsButton>
        </NuxtLinkLocale>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import SvgIcon from '../SvgIcon.vue'
import OdsButton from '../OdsButton.vue'
import { useI18n } from '#imports'
import type { DcatApChV2DistributionAdapter } from './model/dcat-ap-ch-v2-distribution-adapter'
import OdsRelativeDateToggle from '../OdsRelativeDateToggle.vue'

const { t } = useI18n()

interface DistributionProps {
  distribution: DcatApChV2DistributionAdapter
}

const props = defineProps<DistributionProps>()

const hasDownloadUrl = computed(() => {
  const dist = props.distribution

  if (!dist) {
    // we are not ready
    return false
  }
  const downloadUrls = dist.downloadUrls
  if (downloadUrls.length > 0) {
    // we have at least one download url
    return true
  }
  // empty array return false
  return false
})
const hasAccessUrl = computed(() => {
  const dist = props.distribution.accessUrls

  if (!dist) {
    // we are not ready
    return false
  }
  const accessUrls = props.distribution.accessUrls
  if (accessUrls.length > 0) {
    // we have at least one accessUrl url
    return true
  }
  // empty array return false
  return false
})
</script>

<style scoped lang="scss">
.download-item__title {
  text-overflow: ellipsis;
  overflow: hidden;
  white-space: nowrap;
}

.distribution-item {
  display: flex;
  flex-direction: column;
  border-bottom-width: 1px;
  border-bottom-style: solid;
  border-color: var(--color-secondary-200);
  padding-top: 12px;
  padding-bottom: 24px;
}

.footer {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}
.call-to-action {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-top: 12px;
  a {
    display: flex;
    flex-direction: row;
    text-decoration: none;
  }
}
</style>
