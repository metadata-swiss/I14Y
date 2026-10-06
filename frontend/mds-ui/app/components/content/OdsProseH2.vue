<template>
  <ProseH2 v-bind="$attrs" class="h2">
    <slot />
  </ProseH2>
</template>

<style>
h2 {
  margin-top: 3.5rem;
}

h2 a {
  text-decoration: none;
  color: unset;
}

h2 a:hover {
  text-decoration: none;
  color: unset;
}
</style>
