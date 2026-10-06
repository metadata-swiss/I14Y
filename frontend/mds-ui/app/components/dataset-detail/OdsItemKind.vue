<template>
  <span class="label">{{ props.kind }}</span>
</template>

<script setup lang="ts">
interface Props {
  kind: string
}

const props = defineProps<Props>()
</script>

<style lang="scss" scoped>
.label {
  position: relative;
  background-color: #e6f0fa;
  color: #1976d2;
  padding: 10px 20px;
  border-radius: 6px;
  font-weight: 600;
  letter-spacing: 0.03em;
  display: inline-block;
  margin-right: 10px;
  vertical-align: middle;
  border: 1px solid #b3d4fc;
  margin-bottom: 48px;
}
</style>
