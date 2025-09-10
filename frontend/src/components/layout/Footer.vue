<script setup lang="ts">
const props = defineProps<{
  isSidebarCollapsed?: boolean
}>()
</script>

<template>
  <footer class="flex justify-content-center p-2" style="color: var(--p-surface-500)">
    <i
      v-if="isSidebarCollapsed"
      class="pi pi-info-circle text-color-secondary text-sm"
      v-tooltip.right="'© SIGAM v0.1.0'"
    />
    <p v-else class="text-xs">© SIGAM v0.1.0</p>
  </footer>
</template>

<style scoped></style>
