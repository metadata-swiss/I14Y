<template>
  <ProseH3 v-bind="$attrs" class="h3">
    <slot />
  </ProseH3>
</template>

<style>
h3 {
  margin-top: 3.5rem;
}

h3 a {
  text-decoration: none;
  color: unset;
}

h3 a:hover {
  text-decoration: none;
  color: unset;
}
</style>
