<script setup lang="ts">
import ItemSolicitation from './ItemSolicitation.vue'
import { useSolicitationStore } from '../stores/solicitationStore'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue'
import Button from 'primevue/button'
import AddItemDialog from './AddItemDialog.vue'
import { ref } from 'vue'

const props = defineProps<{
  isEditing: boolean
}>()

const solicitationStore = useSolicitationStore()
const { currentSolicitation } = storeToRefs(solicitationStore)
const toast = useToast()
const isAddItemDialogVisible = ref(false)

const handleItemRemoveFromDetails = (itemId: number) => {
  if (!currentSolicitation.value) return
  solicitationStore.removeItem(itemId)
  toast.add({
    severity: 'warn',
    summary: 'Removido',
    detail: `Item removido da solicitação.`,
    life: 3000,
  })
}

</script>

<template>
  <div class="flex flex-column justify-content-between w-full">
    <div v-if="isEditing" class="flex justify-content-end mb-2">
      <Button
        type="button"
        label="Adicionar"
        icon="pi pi-plus"
        size="small"
        text
        @click="isAddItemDialogVisible = true"
      />
    </div>
    <div class="items-list overflow-y-auto">
      <ItemSolicitation
        v-for="item in currentSolicitation?.itens"
        :key="item.catMat"
        :item="item"
        :is-editing="props.isEditing"
        @remove-item="handleItemRemoveFromDetails"
      />
    </div>
  </div>
  <AddItemDialog
    v-model:visible="isAddItemDialogVisible"
    @update:visible="isAddItemDialogVisible = false"
  />
</template>

<style scoped>
.items-list {
  max-height: 480px;
}
</style>
