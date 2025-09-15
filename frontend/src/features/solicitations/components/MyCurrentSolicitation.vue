<script setup lang="ts">
import { computed, inject, ref } from 'vue'
import { Button, useToast } from 'primevue'
import ItemSolicitation from './ItemSolicitation.vue'
import { IftaLabel } from 'primevue'
import Textarea from 'primevue/textarea'
import { useSolicitationCartStore } from '../stores/solicitationCartStore'
import { storeToRefs } from 'pinia'
import { SolicitationContextKey } from '../keys'
import { useSettingStore } from '@/features/settings/stores/settingStore'

const solicitationContext = inject(SolicitationContextKey)

const solicitationCartStore = useSolicitationCartStore()
const { solicitationItems, justification, error, isLoading } = storeToRefs(solicitationCartStore)
const toast = useToast()
const settingStore = useSettingStore()
const { deadline, deadlineHasExpired } = storeToRefs(settingStore)

const isSolicitationValid = (): boolean => {
  if (!solicitationItems.value || solicitationItems.value.length === 0) {
    toast.add({
      severity: 'error',
      summary: 'Nenhum Item',
      detail: 'A solicitação deve conter pelo menos um item.',
      life: 3000,
    })
    return false
  }

  let isValid = true

  if (
    solicitationContext?.isGeneral &&
    (!justification.value || justification.value.trim() === '')
  ) {
    toast.add({
      severity: 'error',
      summary: 'Campo Obrigatório',
      detail: 'A "Justificativa Geral" é obrigatória para solicitações do tipo Geral.',
      life: 3000,
    })
    isValid = false
  }

  for (const item of solicitationItems.value) {
    if (!item.quantidade || item.quantidade <= 0) {
      toast.add({
        severity: 'error',
        summary: 'Quantidade Inválida',
        detail: `O item "${item.nome}" deve ter uma quantidade maior que zero.`,
        life: 3000,
      })
      isValid = false
    }

    if (!item.precoSugerido || item.precoSugerido <= 0) {
      toast.add({
        severity: 'error',
        summary: 'Preço Inválido',
        detail: `O item "${item.nome}" deve ter um preço sugerido maior que zero.`,
        life: 3000,
      })
      isValid = false
    }

    if (
      !solicitationContext?.isGeneral &&
      (!item.justificativa || item.justificativa.trim() === '')
    ) {
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: `A justificativa é obrigatória para o item "${item.nome}" em solicitações patrimoniais.`,
        life: 3000,
      })
      isValid = false
    }
  }

  return isValid
}

const createSolicitation = async () => {
  if (!isSolicitationValid()) return

  const success = await solicitationCartStore.createSolicitation(solicitationContext?.isGeneral)

  if (success) {
    toast.add({
      severity: 'success',
      summary: 'Sucesso!',
      detail: 'Sua solicitação foi enviada.',
      life: 3000,
    })
  } else {
    toast.add({
      severity: 'error',
      summary: 'Erro ao Enviar',
      detail: error || 'Não foi possível criar a solicitação.',
      life: 3000,
    })
  }
}

const disabledSendSolicitation = computed(() => {
  if (solicitationItems.value.length === 0) {
    return true
  }

  if (deadlineHasExpired.value) return true

  if (solicitationContext?.isGeneral) {
    return !justification.value || justification.value.trim() === ''
  } else {
    return solicitationItems.value.some(
      (item) => !item.justificativa || item.justificativa.trim() === '',
    )
  }
})

const handleItemRemove = (itemId: number) => {
  if (!solicitationItems.value || solicitationItems.value.length === 0) return
  solicitationCartStore.removeItem(itemId)
  toast.add({
    severity: 'warn',
    summary: 'Removido',
    detail: `Item removido da solicitação.`,
    life: 3000,
  })
}
</script>

<template>
  <div class="flex flex-column justify-content-between w-full h-full">
    <div v-if="solicitationItems.length > 0" class="items-list overflow-y-auto">
      <ItemSolicitation
        v-for="item in solicitationItems"
        :key="item.catMat"
        :item="item"
        :is-editing="true"
        @remove-item="handleItemRemove"
      />
    </div>
    <div v-else class="flex flex-column align-items-center text-center p-4">
      <i class="pi pi pi-box text-3xl text-color-secondary mb-2"></i>
      <span class="font-bold">Não há itens na sua solicitação.</span>
      <p class="text-color-secondary mt-2">
        Navegue pelo catálogo para encontrar e adicionar os materiais que você precisa.
      </p>
    </div>
    <div class="flex w-full justify-content-end mt-2 gap-2">
      <IftaLabel v-if="solicitationContext?.isGeneral" class="w-full">
        <Textarea
          id="textarea_label"
          class="w-full h-full"
          size="small"
          v-model="justification"
          style="resize: none"
        />
        <label for="textarea_label">Justificativa Geral</label>
      </IftaLabel>
      <div class="flex align-items-end">
        <Button
          label="Solicitar"
          icon="pi pi-send"
          size="small"
          @click="createSolicitation"
          :disabled="disabledSendSolicitation"
          v-tooltip.top="
            deadlineHasExpired ? 'O prazo para submissão de solicitações foi encerrado.' : ''
          "
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.items-list {
  max-height: calc(100vh - 250px);
}
</style>
