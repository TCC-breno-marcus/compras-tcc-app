<script setup lang="ts">
import ItemComponent from './ItemComponent.vue'
import type { Item } from '../types'

defineProps<{
  items: Item[]
}>()

const emit = defineEmits(['viewDetails'])
</script>

<template>
  <div class="items-grid mt-2 gap-2">
    <ItemComponent
      v-for="item in items"
      :key="item.catMat"
      :item="item"
      @viewDetails="emit('viewDetails', item)"
    >
      <template #actions>
        <slot name="actions" :item="item"></slot>
      </template>
    </ItemComponent>
  </div>
</template>

<style scoped>
.items-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  max-height: calc(100vh - 320px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

:deep(.p-toolbar-start) {
  flex: 1 1 auto;
}
</style>
