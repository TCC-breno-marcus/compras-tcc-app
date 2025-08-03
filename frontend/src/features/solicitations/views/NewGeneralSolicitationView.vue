<script setup lang="ts">
import Divider from 'primevue/divider'
import ItemsListComponent from '../components/ItemsListComponent.vue'
import MySolicitation from '../components/MySolicitation.vue'
import { useLayoutStore } from '@/stores/layout'
import { ref, computed } from 'vue'
import CatalogoBrowser from '@/features/catalogo/components/CatalogoBrowser.vue'

const layoutStore = useLayoutStore()
const isMobileView = computed(() => ['xs', 'sm'].includes(layoutStore.currentBreakpoint))
</script>

<template>
  <div class="flex flex-column w-full h-full justify-content-between align-items-center p-2">
    <div class="flex flex-column md:flex-row p-2">
      <div class="flex flex-column align-content-end w-full md:w-7">
        <div class="flex justify-content-between align-items-center mb-2">
          <h3>Catálogo de Materiais</h3>
        </div>
        <!-- <ItemsListComponent /> -->
         <CatalogoBrowser />
      </div>

      <Divider :layout="isMobileView ? 'horizontal' : 'vertical'" />

      <div class="flex flex-column align-content-end w-full md:w-5">
        <h3 class="mb-3">Sua Solicitação</h3>
        <MySolicitation />
      </div>
    </div>

    <div class="flex">
      <p class="text-xs text-center" style="color: var(--p-surface-500)">
        <i class="pi pi pi-info-circle text-xs"></i>
        Todas as imagens utilizadas são meramente ilustrativas.
      </p>
    </div>
  </div>
</template>
