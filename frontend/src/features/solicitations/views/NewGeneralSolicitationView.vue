<script setup lang="ts">
import Divider from 'primevue/divider'
import MyCurrentSolicitation from '../components/MyCurrentSolicitation.vue'
import { useLayoutStore } from '@/stores/layout'
import { ref, computed, provide, reactive, readonly } from 'vue'
import CatalogoBrowser from '@/features/catalogo/components/CatalogoBrowser.vue'
import type { Item } from '@/features/catalogo/types'
import { useSolicitationStore } from '../stores/solicitationStore'
import { Button, useConfirm } from 'primevue'
import { useToast } from 'primevue/usetoast'
import { storeToRefs } from 'pinia'
import { useLeaveConfirmation } from '@/composables/useLeaveConfirmation'
import { DISCARD_SOLICITATION_CONFIRMATION } from '@/utils/confirmationFactoryUtils'

const solicitationContext = reactive({
  dialogMode: 'selection',
  isGeneral: true
});

provide('solicitationContext', readonly(solicitationContext));

const layoutStore = useLayoutStore()
const isMobileView = computed(() => ['xs', 'sm'].includes(layoutStore.currentBreakpoint))

const toast = useToast()
const confirm = useConfirm()
const solicitationStore = useSolicitationStore()
const { solicitationItems, justification } = storeToRefs(solicitationStore)
const catalogoBrowserRef = ref()

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

  if (catalogoBrowserRef.value) {
    catalogoBrowserRef.value.closeDialog()
  }
}

const resetSolicitation = () => {
  confirm.require({
    ...DISCARD_SOLICITATION_CONFIRMATION,
    accept: () => {
      solicitationStore.clearSolicitation()
    },
  })
}

const isSolicitationDirty = computed(() => {
  return solicitationItems.value.length > 0 || justification.value.trim() !== ''
})

useLeaveConfirmation(isSolicitationDirty)

</script>

<template>
  <div class="flex flex-column w-full h-full justify-content-between align-items-center p-2">
    <div>
      <h2>Criar Solicitação Geral</h2>
    </div>

    <div class="flex flex-column md:flex-row p-2">
      <div class="flex flex-column align-content-end w-full md:w-7">
        <div class="flex justify-content-between align-items-center mb-2">
          <h3>Buscar Itens</h3>
        </div>
        <!-- Na listagem do catalogo nas telas de criar solicitação, deve filtrar
         por padrão somente itens ativos -->
        <CatalogoBrowser ref="catalogoBrowserRef">
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
            v-if="isSolicitationDirty"
            icon="pi pi-eraser"
            severity="danger"
            label="Limpar Solicitação"
            v-tooltip="`Limpar Solicitação`"
            size="small"
            text
            @click="resetSolicitation"
          />
        </div>
        <MyCurrentSolicitation />
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
