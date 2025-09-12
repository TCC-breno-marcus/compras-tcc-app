<script setup lang="ts">
import { ref, onMounted } from 'vue'
import CatalogoBrowser from '@/features/catalogo/components/CatalogoBrowser.vue'
import Tag from 'primevue/tag'
import { useCategoriaStore } from '@/features/catalogo/stores/categoriaStore'
import { CATEGORY_ITEMS_GENERAL, CATEGORY_ITEMS_PATRIMONIALS } from '@/features/solicitations/constants'

const categoriaStore = useCategoriaStore()

onMounted(() => {
  categoriaStore.fetch()
})

const catalogoBrowserRef = ref()
</script>

<template>
  <CatalogoBrowser
    ref="catalogoBrowserRef"
    :category-names="[...CATEGORY_ITEMS_GENERAL, ...CATEGORY_ITEMS_PATRIMONIALS]"
  >
    <template #actions="{ item }">
      <Tag
        :severity="item.isActive ? 'success' : 'danger'"
        :value="item.isActive ? 'Ativo' : 'Inativo'"
      ></Tag>
    </template>
  </CatalogoBrowser>
</template>

<style scoped></style>
