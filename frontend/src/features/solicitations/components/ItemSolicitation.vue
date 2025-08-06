<script setup lang="ts">
import { inject, ref } from 'vue'
import Button from 'primevue/button'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import { FloatLabel } from 'primevue'
import type { SolicitationContext, SolicitationItem } from '../types'
import { useSolicitationStore } from '../stores/solicitationStore'
import { useToast } from 'primevue/usetoast'

const props = defineProps<{
  item: SolicitationItem
}>()

const solicitationContext = inject<SolicitationContext>('solicitationContext')

const solicitationStore = useSolicitationStore()
const toast = useToast()

const removeItem = () => {
  solicitationStore.removeItem(props.item.id)
  toast.add({
    severity: 'warn',
    summary: 'Removido',
    detail: `Item ${props.item.nome} (${props.item.catMat}) removido da solicitação.`,
    life: 3000,
  })
}

const onQuantityChange = (newQuantity: number) => {
  const item = props.item
  if (item.quantity === null || item.quantity === undefined || item.quantity < 1) {
    solicitationStore.updateItemQuantity(item.id, 1)
    return
  }
  solicitationStore.updateItemQuantity(props.item.id, newQuantity)
}
</script>
<template>
  <div
    class="flex md:flex-column lg:flex-row justify-content-between lg:justify-content-between p-2 w-full gap-2"
    :class="solicitationContext?.isGeneral ? 'item-card mb-2' : 'pb-0'"
  >
    <div class="flex justify-content-start align-items-center gap-2 mr-2">
      <div class="image-preview-container flex justify-content-center align-items-center">
        <img v-if="item.linkImagem" :src="item.linkImagem" :alt="item.nome" class="item-image" />
        <div v-else class="image-placeholder">
          <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
        </div>
      </div>
      <div>
        <p class="font-bold text-sm">{{ item.nome }}</p>
        <p class="text-xs text-color-secondary">CATMAT {{ item.catMat }}</p>
      </div>
    </div>

    <div class="flex align-items-center gap-2 justify-content-end">
      <FloatLabel variant="on" class="quantity-input w-full sm:w-3">
        <InputNumber
          v-model="item.quantity"
          inputId="on_label_qtde"
          :min="1"
          :max="9999"
          size="small"
          fluid
          class="w-full"
          inputClass="w-full"
        />
        <label for="on_label_qtde">Qtde.</label>
      </FloatLabel>
      <FloatLabel variant="on" class="price-input">
        <InputNumber
          v-model="item.precoSugerido"
          inputId="on_label_price"
          mode="currency"
          currency="BRL"
          locale="pt-BR"
          fluid
          :min="0"
          size="small"
        />
        <label for="on_label_price">Preço Unitário</label>
      </FloatLabel>
      <Button
        icon="pi pi-trash"
        variant="text"
        severity="danger"
        size="small"
        @click="removeItem"
      />
    </div>
  </div>
  <div
    v-if="!solicitationContext?.isGeneral"
    class="item-card flex justify-content-end w-full pb-2 pr-6 pl-7 md:pt-2 lg:pt-0"
  >
    <FloatLabel variant="on" class="w-full">
      <InputText
        v-model="item.justification"
        inputId="on_label_justification"
        :invalid="item.justification === ''"
        size="small"
        class="w-full"
        inputClass="w-full"
        :maxlength="250"
      />
      <label for="on_label_justification">Justificativa</label>
    </FloatLabel>
  </div>
</template>

<style scoped>
.item-card {
  /* background-color: var(--p-surface-50); */
  border-bottom: 1px solid var(--p-surface-200);
}

.p-dark .item-card {
  border-bottom: 1px solid var(--p-surface-800);
  background-color: initial;
}

.image-preview-container {
  position: relative;
  cursor: pointer;
  overflow: hidden;
  height: 48px;
  width: 48px;
  /* background-color: var(--p-surface-50); */
}

.p-dark .image-preview-container {
  background-color: initial;
}

.item-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
  transition: transform 0.4s ease;
}

.placeholder-icon {
  font-size: 1.5rem;
  padding: 15px;
  /* color: var(--p-surface-500);  */
}

.quantity-input {
  width: 5rem !important;
}

.price-input {
  width: 7rem !important;
}
</style>
