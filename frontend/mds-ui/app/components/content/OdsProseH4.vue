<template>
  <ProseH4 v-bind="$attrs" class="h3">
    <slot />
  </ProseH4>
</template>

<style>
h4 {
  margin-top: 3.5rem;
}

h4 a {
  text-decoration: none;
  color: unset;
}

h4 a:hover {
  text-decoration: none;
  color: unset;
}
</style>
