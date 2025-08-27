<script setup lang="ts">
import Divider from 'primevue/divider'
import MyCurrentSolicitation from '../components/MyCurrentSolicitation.vue'
import { useLayoutStore } from '@/stores/layout'
import { ref, computed, inject } from 'vue'
import CatalogoBrowser from '@/features/catalogo/components/CatalogoBrowser.vue'
import type { Item } from '@/features/catalogo/types'
import { useSolicitationCartStore } from '../stores/solicitationCartStore'
import { Button, useConfirm } from 'primevue'
import { useToast } from 'primevue/usetoast'
import { storeToRefs } from 'pinia'
import { useLeaveConfirmation } from '@/composables/useLeaveConfirmation'
import { DISCARD_SOLICITATION_CONFIRMATION } from '@/utils/confirmationFactoryUtils'
import CustomBreadcrumb from '@/components/ui/CustomBreadcrumb.vue'
import { CATEGORY_ITEMS_GENERAL, CATEGORY_ITEMS_PATRIMONIALS } from '../constants'
import { SolicitationContextKey } from '../keys'

const solicitationContext = inject(SolicitationContextKey)

const layoutStore = useLayoutStore()
const { currentBreakpoint } = storeToRefs(layoutStore)
const isMobileView = computed(() => {
  return ['xs', 'sm'].includes(currentBreakpoint.value)
})

const toast = useToast()
const confirm = useConfirm()
const solicitationCartStore = useSolicitationCartStore()
const { solicitationItems, justification } = storeToRefs(solicitationCartStore)
const catalogoBrowserRef = ref()

const addItemSolicitation = (item: Item) => {
  const actionReturn = solicitationCartStore.addItem(
    item,
    solicitationContext?.isGeneral ? 'geral' : 'patrimonial',
  )
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

  if (catalogoBrowserRef.value) {
    catalogoBrowserRef.value.closeDialog()
  }
}

const resetSolicitation = () => {
  confirm.require({
    ...DISCARD_SOLICITATION_CONFIRMATION,
    accept: () => {
      solicitationCartStore.$reset()
    },
  })
}

const hasUnsavedChanges = computed(() => {
  return solicitationItems.value.length > 0 || justification.value.trim() !== ''
})

useLeaveConfirmation()
</script>

<template>
  <div class="flex flex-column w-full h-full align-items-center">
    <div class="w-full">
      <CustomBreadcrumb />
    </div>

    <div class="flex flex-column md:flex-row p-2">
      <div class="flex flex-column align-content-end w-full md:w-7">
        <div class="flex justify-content-between align-items-center mb-2">
          <h3>Buscar Itens</h3>
        </div>
        <CatalogoBrowser
          ref="catalogoBrowserRef"
          :category-names="
            solicitationContext?.isGeneral ? CATEGORY_ITEMS_GENERAL : CATEGORY_ITEMS_PATRIMONIALS
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

      <Divider :layout="isMobileView ? 'horizontal' : 'vertical'" />

      <div class="flex flex-column align-content-end w-full md:w-5">
        <div class="flex justify-content-between align-items-center w-full mb-4">
          <h3>Sua Solicitação</h3>
          <Button
            v-if="hasUnsavedChanges"
            icon="pi pi-eraser"
            severity="danger"
            label="Limpar Solicitação"
            size="small"
            text
            @click="resetSolicitation"
          />
        </div>
        <MyCurrentSolicitation />
      </div>
    </div>
  </div>
</template>
