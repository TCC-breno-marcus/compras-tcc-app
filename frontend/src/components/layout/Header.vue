<script setup lang="ts">
import Button from 'primevue/button';
import Avatar from 'primevue/avatar';
import { useLayoutStore } from '@/stores/layout';

const layoutStore = useLayoutStore();

function toggleSidebar() {
  layoutStore.toggleSidebar();
}
</script>

<template>
  <div class="flex align-items-center justify-content-between p-2">
    <Button icon="pi pi-bars" variant="text" style="color: var(--p-text-color);" @click="toggleSidebar" />
    <div class="flex align-items-center">
      <Avatar label="J" class="mr-2" shape="circle" />
      <p>João da Silva (DCOMP)</p>
    </div>
  </div>
</template>

<style scoped></style>
