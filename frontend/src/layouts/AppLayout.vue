<script setup lang="ts">
import { RouterView } from 'vue-router'
import Header from '@/components/layout/Header.vue'
import SidebarMenu from '@/components/layout/SidebarMenu.vue'
import Footer from '@/components/layout/Footer.vue'
import { useBreakpoint } from '@/composables/useBreakpoint';

const { isLargeScreen } = useBreakpoint();

</script>

<template>
  <div class="flex w-full">

    <SidebarMenu v-if="isLargeScreen" />

    <div class="flex flex-column justify-content-between align-items-between w-full h-screen">
      <Header />
      <main class="main-container flex flex-column h-full px-4 py-2">
        <RouterView />
      </main>
      <Footer />
    </div>

  </div>
</template>

<style scoped>
.main-container {
  background-color: white;
  overflow-y: auto;
}

.p-dark .main-container {
  background-color: initial;
}
</style>