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

</style>
