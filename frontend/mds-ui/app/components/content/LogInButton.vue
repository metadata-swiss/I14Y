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
import { useLoginWithRedirect } from '@/composables/login'

const login = useLoginWithRedirect()
</script>
