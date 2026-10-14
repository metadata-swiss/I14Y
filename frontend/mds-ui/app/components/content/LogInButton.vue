<template>
  <ods-button
    variant="outline"
    @click="login"
  >
    <slot />
  </ods-button>
</template>

<script setup lang="ts">
import OdsButton from '../OdsButton.vue'

// Used by CMS pages (`:::LogInButton`). mds-ui has no login of its own, so it opens the I14Y input portal,
// like the login button of the top header.
const config = useRuntimeConfig()

function login() {
  navigateTo(config.public.loginUrl, { external: true })
}
</script>
