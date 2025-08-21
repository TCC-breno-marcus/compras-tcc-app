<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { ref } from 'vue'
import CatalogoBrowser from '@/features/catalogo/components/CatalogoBrowser.vue'
import { CATEGORY_ITEMS_GENERAL, CATEGORY_ITEMS_PATRIMONIALS } from '../constants'
import type { Item } from '@/features/catalogo/types'
import { useToast } from 'primevue'
import { storeToRefs } from 'pinia'
import { useSolicitationStore } from '../stores/solicitationStore'

const props = defineProps<{
  visible: boolean
}>()

const isLoading = ref(false)
const catalogoBrowserRef = ref()
const toast = useToast()
const solicitationStore = useSolicitationStore()
const { currentSolicitation } = storeToRefs(solicitationStore)

const emit = defineEmits(['update:visible'])

const closeModal = () => {
  emit('update:visible', false)
}

const addItemSolicitation = (item: Item) => {
  const actionReturn = solicitationStore.addItem(item)
  if (actionReturn === 'added') {
    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: `Item ${item.nome} (${item.catMat}) adicionado à solicitação.`,
      life: 3000,
    })
  } else if (actionReturn === 'incremented') {
    toast.add({
      severity: 'info',
      summary: 'Atualizado',
      detail: `Quantidade do item ${item.nome} (${item.catMat}) atualizada.`,
      life: 3000,
    })
  }

  closeModal()
}
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Adicionar Item à Solicitação`"
    :style="{ width: '90vw', maxWidth: '800px' }"
  >
    <div class="dialog-content flex flex-column md:flex-row">
      <CatalogoBrowser
        ref="catalogoBrowserRef"
        :category-names="
          currentSolicitation?.justificativaGeral !== ''
            ? CATEGORY_ITEMS_GENERAL
            : CATEGORY_ITEMS_PATRIMONIALS
        "
      >
        <template #actions="{ item }">
          <Button
            icon="pi pi-plus"
            rounded
            severity="success"
            aria-label="Adicionar à Solicitação"
            v-tooltip="`Adicionar à Solicitação`"
            size="small"
            @click="addItemSolicitation(item)"
          />
        </template>
        <template #dialog-actions="{ item }">
          <Button
            v-if="item"
            icon="pi pi-plus"
            severity="success"
            label="Adicionar à Solicitação"
            v-tooltip="`Adicionar à Solicitação`"
            size="small"
            @click="addItemSolicitation(item)"
          />
        </template>
      </CatalogoBrowser>
    </div>

    <template #footer>
      <Button
        label="Cancelar"
        severity="danger"
        icon="pi pi-times"
        @click="closeModal"
        text
        size="small"
        :disabled="isLoading"
      />
    </template>
  </Dialog>
</template>

<style scoped></style>
