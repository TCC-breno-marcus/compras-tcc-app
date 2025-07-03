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
  <div class="header-container flex align-items-center justify-content-between p-2">
    <Button icon="pi pi-bars" variant="text" style="color: var(--p-text-color);" @click="toggleSidebar" />
    <div class="flex align-items-center">
      <Avatar label="J" class="mr-2" shape="circle" />
      <p class="text-sm">João da Silva (DCOMP)</p>
    </div>
  </div>
</template>

<style scoped>

/* .header-container {
  background: linear-gradient(to bottom, transparent 0%, var(--p-surface-50) 100%);
} */

/* OU */


.header-container {
  position: relative;
}
.header-container::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0%;
  right: 0%; 
  height: 1px;
  background: linear-gradient(to right, transparent 0%, var(--p-surface-200) 50%, transparent 100%);
} 

</style>
